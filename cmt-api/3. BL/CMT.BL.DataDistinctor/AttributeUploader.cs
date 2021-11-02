using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using CMT.BO;
using CMT.BO.Metadata;
using CMT.Common;
using CMT.DL;
using CMT.DL.Core;
using DDR.Common;
using DDR.Files.Configuration;
using DDR.Files.Tabular;
using DDR.Files.Tabular.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Transactions;

namespace CMT.BL.DataDistinctor
{
    public class AttributeUploader : BaseUploader
    {
        private const int BatchSize = 5000;
        private static readonly string[] globalBrandsToSkip = { "Undefined" };
        private AWSConfigurationItem awsConfig => ApplicationSettings.AttributeUploaderConfiguration;
        private CMTConfig cmtConfig => ApplicationSettings.CMTConfig;
        public AttributeUploader()
        {
        }

        public void ImportFile(string key)
        {

            //using (DbConnectionScope.Create(ConfigurationManager.ConnectionStrings["CMTEntitiesConnectionString"].ConnectionString))
            {
                CmtEntities dbContext = Container.GetInstance<CmtEntities>();
                dbContext.Database.CommandTimeout = ApplicationSettings.CommandTimeout;

                using (ElementManager elementManager = Container.GetInstance<ElementManager>())
                {
                    using (ValueManager valueManager = Container.GetInstance<ValueManager>())
                    {
                        using (ValueDetailManager valueDetailManager = Container.GetInstance<ValueDetailManager>())
                        {
                            using (BrandImportHistoryManager brandImportHistoryManager = Container.GetInstance<BrandImportHistoryManager>())
                            {
                                using (CountryManager countryManager = Container.GetInstance<CountryManager>())
                                {
                                    using (ValueListManager valueListManager = Container.GetInstance<ValueListManager>())
                                    {
                                        string fileName;
                                        List<ProductPackData> productPackList = GetData(key, cmtConfig.ProductPackConfig, out fileName);


                                        Dictionary<Guid, List<string>> valuesForTypes = new Dictionary<Guid, List<string>>();
                                        using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                                        {
                                            List<string> customAttributeValues;
                                            foreach (ProductPackDataRowConfig rowConfig in cmtConfig.ProductPackConfig.RowConfigs)
                                            {
                                                customAttributeValues = productPackList
                                                    .SelectMany(o => o.CustomAttributes)
                                                    .Where(x => x.MetaDataElementId == rowConfig.MetaDataElementId)
                                                    .Select(x => x.Value)
                                                    .Distinct()
                                                    .ToList();
                                                valuesForTypes.Add(rowConfig.MetaDataElementId, customAttributeValues);
                                                ImportValuesForElement(elementManager, valueManager, valueListManager, customAttributeValues, rowConfig.MetaDataElementId);
                                            }

                                            transactionScope.Complete();

                                        }

                                        ElementBO brandElement = elementManager.GetBrandElement();
                                        using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                                        {
                                            if (brandElement.ValueListId != null)
                                            {

                                                foreach (ProductPackDataRowConfig rowConfig in cmtConfig.ProductPackConfig.RowConfigs)
                                                {
                                                    ImportAttributesForElement(valueManager, valueDetailManager, elementManager, productPackList, brandElement, rowConfig.MetaDataElementId);
                                                }
                                                ImportGlobalCodeForElement(valueManager, productPackList, brandElement);

                                            }

                                            BrandImportHistoryBO brandImportHistory = new BrandImportHistoryBO()
                                            {
                                                FileName = fileName,
                                                Date = DateTime.Now.ToUniversalTime().Date
                                            };

                                            brandImportHistoryManager.InsertObject(brandImportHistory);

                                            transactionScope.Complete();
                                        }

                                    }
                                }
                            }
                        }
                    }

                }
            }

        }

        private static void ImportValuesForElement(ElementManager elementManager, ValueManager valueManager, ValueListManager valueListManager, List<string> valuesFromFile, Guid metaDataElementId)
        {


            ElementBO element = elementManager.GetElementById(metaDataElementId);
            if (valuesFromFile.Any())
            {

                List<ValueBO> valuesToInsert = new List<ValueBO>();
                List<ValueBO> valuesToUpdate = new List<ValueBO>();

                if (element.ValueListId == null)
                {

                    ValueListBO valueList = new ValueListBO()
                    {
                        Name = string.Format("{0} Value List", element.Name)
                    };

                    valueListManager.InsertObject(valueList);

                    element.ValueListId = valueList.ObjectId;
                    elementManager.UpdateObject(element);
                }
                List<ValueBO> values = valueManager.GetObjectsByValueListId(element.ValueListId.Value).ToList();
                List<string> flattenList = FlattenValueList(valuesFromFile);
                foreach (string attributeTextValue in flattenList)
                {




                    ValueBO existingValue = values.Where(o => o.TextValue.ToLowerInvariant() == attributeTextValue.ToLowerInvariant()).FirstOrDefault();

                    if (existingValue == null)
                    {
                        ValueBO newValueBo = new ValueBO();
                        newValueBo.TextValue = attributeTextValue;
                        newValueBo.ValueListId = element.ValueListId;
                        newValueBo.Status = (byte)ValueStatus.Active;
                        valuesToInsert.Add(newValueBo);

                    }
                    else
                    {
                        if (existingValue.Status == (byte)ValueStatus.Inactive)
                        {
                            existingValue.Status = (byte)ValueStatus.Active;
                            existingValue.TextValue = attributeTextValue;
                            valuesToUpdate.Add(existingValue);
                        }
                        else if (existingValue.TextValue != attributeTextValue && !valuesToUpdate.Any(o => o.ObjectId == existingValue.ObjectId))
                        {
                            //take latest text value from file (change can be capital case)
                            existingValue.TextValue = attributeTextValue;
                            valuesToUpdate.Add(existingValue);
                        }

                    }


                }
                valueManager.InsertObjects(valuesToInsert);
                //deactivae values which are not on list
                values = valueManager.GetObjectsByValueListId(element.ValueListId.Value).ToList();
                using (ValueTagManager valueTagManager = Container.GetInstance<ValueTagManager>())
                {

                    List<Guid> tagsToRemove = new List<Guid>();
                    foreach (ValueBO v in values)
                    {
                        if (!flattenList.Any(o => o.ToLowerInvariant() == v.TextValue.ToLowerInvariant()))
                        {
                            v.Status = (byte)ValueStatus.Inactive;
                            tagsToRemove.Add(v.ObjectId);
                            valuesToUpdate.Add(v);
                        }

                    }

                    valueManager.UpdateObjects(valuesToUpdate);
                    foreach (Guid tagValueId in tagsToRemove)
                    {
                        valueTagManager.RemoveTagsByValueId(tagValueId);
                    }
                }

            }
        }

        private static List<string> FlattenValueList(List<string> valuesFromFile)
        {
            List<string> flatList = new List<string>();
            foreach (string attributeTextValues in valuesFromFile)
            {
                string[] splittedAttributes = attributeTextValues.Split(',');
                flatList.AddRange(splittedAttributes);
            }
            return flatList.Select(o => o.Trim()).Distinct().ToList();
        }
        private static List<string> SplitValueString(string valueFromFile)
        {
            List<string> flatList = new List<string>();
            if (!string.IsNullOrEmpty(valueFromFile))
            {
                string[] splittedAttributes = valueFromFile.Split(',');
                flatList.AddRange(splittedAttributes);

            }
            return flatList.Select(o => o.Trim()).Distinct().ToList();
        }


        private static void ImportAttributesForElement(ValueManager valueManager, ValueDetailManager valueDetailManager, ElementManager elementManager, List<ProductPackData> productPackList, ElementBO brandElement, Guid metaDataElementId)
        {
            ElementBO element = elementManager.GetElementById(metaDataElementId);
            List<ValueBO> elementValues = valueManager.GetObjectsByValueListId(element.ValueListId.Value).ToList();

            List<ValueBO> globalValues = valueManager.GetParentObjectsByValueListId(brandElement.ValueListId.Value).ToList();
            List<ValueDetailBO> valueDetailsToInsert = new List<ValueDetailBO>();
            List<ValueDetailBO> valueDetailsToUpdate = new List<ValueDetailBO>();
            List<ValueDetailBO> valueDetailsToDelete = new List<ValueDetailBO>();

            foreach (ProductPackData data in productPackList)
            {
                List<ValueBO> brandList = globalValues.Where(o => o.TextValue == data.GlobalPfizerBrandName).ToList();
                foreach (ValueBO brand in brandList)
                {
                    ValueDetailBO valueAttribute = null;

                    List<ValueDetailBO> details = valueDetailManager.GetObjectsUsingBOPredicate(o => o.ValueId == brand.ObjectId && o.Type == ValueDetailType.ValueAttribute && o.AttributeElementId == element.ObjectId);

                    string valueFromFile = data.CustomAttributes.FirstOrDefault(o => o.MetaDataElementId == metaDataElementId).Value;
                    List<string> attributeValuesFromFile = SplitValueString(valueFromFile);

                    //remove attributes for not active values from metadataelement
                    foreach (ValueDetailBO existingDetail in details)
                    {
                        ValueBO attributeValue = elementValues.Where(o => o.ObjectId == existingDetail.AttributeValueId).Single();
                        if (attributeValue != null && attributeValue.Status != (byte)ValueStatus.Active)
                        {
                            valueDetailsToDelete.Add(existingDetail);
                        }
                        //if attribute is removed from file delete existing from db
                        if (!attributeValuesFromFile.Where(o => o.ToLowerInvariant() == attributeValue.TextValue.ToLowerInvariant()).Any())
                        {
                            valueDetailsToDelete.Add(existingDetail);
                        }
                    }

                    foreach (string singleAttribute in attributeValuesFromFile)
                    {
                        ValueBO attributeValueFromElement = elementValues.Where(o => o.TextValue.ToLowerInvariant() == singleAttribute.ToLowerInvariant()).Single();
                        valueAttribute = details.Where(o => o.AttributeValueId == attributeValueFromElement.ObjectId).FirstOrDefault();
                        if (valueAttribute == null)
                        {
                            ValueDetailBO detailBo = new ValueDetailBO();
                            detailBo.Type = ValueDetailType.ValueAttribute;
                            detailBo.ValueId = brand.ObjectId;
                            detailBo.AttributeElementId = element.ObjectId;
                            detailBo.AttributeValueId = attributeValueFromElement.ObjectId;
                            valueDetailsToInsert.Add(detailBo);
                        }
                    }
                }
            }

            valueDetailManager.UpdateObjects(valueDetailsToUpdate);
            valueDetailManager.InsertObjects(valueDetailsToInsert);
            valueDetailManager.DeleteObjects(valueDetailsToDelete);
        }
        private static void ImportGlobalCodeForElement(ValueManager valueManager, List<ProductPackData> productPackList, ElementBO brandElement)
        {

            List<ValueBO> globalValues = valueManager.GetParentObjectsByValueListId(brandElement.ValueListId.Value).ToList();
            List<ValueBO> valuesToUpdate = new List<ValueBO>();

            foreach (ProductPackData data in productPackList)
            {
                List<ValueBO> brandList = globalValues.Where(o => o.TextValue == data.GlobalPfizerBrandName).ToList();
                foreach (ValueBO brand in brandList)
                {

                    brand.GlobalCode = data.GlobalCode;
                    valuesToUpdate.Add(brand);
                }

            }

            valueManager.UpdateObjects(valuesToUpdate);

        }

        public List<string> GetFileNamesToImport(int interval = 0)
        {
            List<string> result = new List<string>();

            using (BrandImportHistoryManager brandImportHistoryManager = new BrandImportHistoryManager())
            {
                DateTime currentDate = DateTime.Now.ToUniversalTime().Date.AddDays(interval);


                List<BrandImportHistoryBO> todayImports = brandImportHistoryManager.GetObjectsUsingBOPredicate(o => o.Date == currentDate && o.FileName.Contains("ProductPack"));

                using (AmazonS3Client client = new AmazonS3Client(awsConfig.AccessKey, awsConfig.SecretKey, RegionEndpoint.GetBySystemName(awsConfig.Region)))
                {
                    S3DirectoryInfo directoryInfo = !string.IsNullOrEmpty(awsConfig.Directory) ? new S3DirectoryInfo(client, awsConfig.BucketName, awsConfig.Directory) : new S3DirectoryInfo(client, awsConfig.BucketName);
                    S3FileInfo[] fileInfos = directoryInfo.GetFiles(string.Format(cmtConfig.ProductPackConfig.SearchPatternTemplate, currentDate.ToString("yyyyMMdd")), SearchOption.AllDirectories);

                    foreach (S3FileInfo fileInfo in fileInfos)
                    {
                        if (todayImports.All(o => o.FileName != fileInfo.Name))
                        {
                            result.Add(fileInfo.FullName.Substring(awsConfig.BucketName.Length + (!string.IsNullOrEmpty(awsConfig.Directory) ? awsConfig.Directory.Length + 1 : 0) + 2));
                        }
                    }
                }
            }

            return result;
        }

        private List<ProductPackData> GetData(string key, ProductPackDataConfig config, out string fileName)
        {
            List<ProductPackData> result = new List<ProductPackData>();

            using (AmazonS3Client client = new AmazonS3Client(awsConfig.AccessKey, awsConfig.SecretKey, RegionEndpoint.GetBySystemName(awsConfig.Region)))
            {
                S3DirectoryInfo directoryInfo = !string.IsNullOrEmpty(awsConfig.Directory) ? new S3DirectoryInfo(client, awsConfig.BucketName, awsConfig.Directory) : new S3DirectoryInfo(client, awsConfig.BucketName);
                S3FileInfo fileInfo = directoryInfo.GetFile(key);

                using (Stream stream = fileInfo.OpenRead())
                {
                    if (fileInfo.Extension == "gz")
                    {
                        using (Stream decompressedStream = new MemoryStream())
                        {
                            using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
                            {
                                gzipStream.CopyTo(decompressedStream);
                                decompressedStream.Position = 0;

                                result = GetDataInternal(decompressedStream, config);
                            }
                        }
                    }
                    else
                    {
                        result = GetDataInternal(stream, config);
                    }
                }

                fileName = fileInfo.Name;
            }

            return result;
        }

        private List<ProductPackData> GetDataInternal(Stream stream, ProductPackDataConfig config)
        {
            List<ProductPackData> result = new List<ProductPackData>();

            using (TabularDataReader reader = TabularFileReaderFactory.Create(stream, DataFormat.Txt, false))
            {
                List<Row> rows;
                do
                {
                    rows = reader.ReadRows(BatchSize).ToList();
                    foreach (Row row in rows)
                    {
                        if (row.Values[4] != null)
                        {
                            ProductPackData brandData = ProductPackData.Create(row.Values, config);

                            if (brandData != null)
                            {
                                result.Add(brandData);
                            }
                        }
                    }

                } while (rows.Any());

                return result;
            }
        }
    }
}
