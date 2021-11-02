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
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Transactions;

namespace CMT.BL.DataDistinctor
{


    public class BrandUploader : BaseUploader
    {


        private const int BatchSize = 5000;
        private const string searchPatternTemplate = "*_PfizerBrand_{0}_*.*";

        private AWSConfigurationItem configuration => ApplicationSettings.BrandUploaderConfiguration;

        public BrandUploader()
        {
        }

        public void ImportFile(string key)
        {
            using (new DbContextScope<CmtEntities>())
            {
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

                                        using (ValueTagManager valueTagManager = Container.GetInstance<ValueTagManager>())
                                        {
                                            string fileName;
                                            List<BrandData> brandDatas = GetData(key, out fileName);
                                            string countryCode = fileName.Split('_').First();
                                            CountryBO country = countryManager.GetObjectsUsingBOPredicate(o => o.Code == countryCode).SingleOrDefault();

                                            if (country != null)
                                            {
                                                Guid countryId = country.ObjectId;
                                                ElementBO brandElement = elementManager.GetBrandElement();

                                                if (brandElement.ValueListId == null)
                                                {
                                                    ValueListBO valueList = new ValueListBO()
                                                    {
                                                        Name = string.Format("{0} Value List", brandElement.Name)
                                                    };

                                                    valueListManager.InsertObject(valueList);

                                                    brandElement.ValueListId = valueList.ObjectId;
                                                    elementManager.UpdateObject(brandElement);
                                                }

                                                List<ValueBO> values = valueManager.GetObjectsByValueListIdAndCountryid(brandElement.ValueListId.Value, countryId).ToList();
                                                List<ValueBO> parentValues = valueManager.GetParentObjectsByValueListId(brandElement.ValueListId.Value).ToList();

                                                List<ValueBO> valuesToInsert = new List<ValueBO>();
                                                List<ValueBO> valuesToUpdate = new List<ValueBO>();
                                                Dictionary<string, string> processedValues = new Dictionary<string, string>();
                                                List<ValueBO> valuesDetailsToInsert = new List<ValueBO>();
                                                List<ValueBO> valuesDetailsToUpdate = new List<ValueBO>();


                                                foreach (BrandData brandData in brandDatas)
                                                {
                                                    ValueBO globalValue = parentValues.SingleOrDefault(o => !o.CountryId.HasValue && string.Equals(o.ExternalId, brandData.GlobalPfizerBrandId));

                                                    if (globalValue == null)
                                                    {
                                                        ValueBO pfizerBrand = valueManager.GetObjectsUsingBOPredicate(o => o.ExternalId == brandData.PfizerBrandId && o.TextValue == brandData.PfizerBrandName && o.CountryId == countryId).LastOrDefault();

                                                        if (pfizerBrand != null)
                                                        {
                                                            globalValue = parentValues.SingleOrDefault(o => !o.CountryId.HasValue && o.ExternalId == brandData.GlobalPfizerBrandId
                                                                                && o.ObjectId == pfizerBrand.ParentId);

                                                       
                                                        }
                                                        else
                                                        {
                                                            globalValue = parentValues.Where(o => !o.CountryId.HasValue && o.ExternalId == brandData.GlobalPfizerBrandId).OrderBy(o => o.CreateDate).LastOrDefault();
                                                        }
                                                    }

                                                    ValueBO localValue = values.SingleOrDefault(o => string.Equals(o.ExternalId, brandData.PfizerBrandId));

                                                    if (globalValue == null && brandData.GlobalPfizerBrandId != "0" && !processedValues.ContainsKey(brandData.GlobalPfizerBrandId))
                                                    {
                                                        globalValue = new ValueBO()
                                                        {
                                                            TextValue = brandData.GlobalPfizerBrandName,
                                                            ExternalId = brandData.GlobalPfizerBrandId,
                                                            //ValueListId = brandElement.ValueListId.Value,
                                                            Status = (byte)ValueStatus.Active
                                                        };

                                                        valuesToInsert.Add(globalValue);
                                                        processedValues.Add(brandData.GlobalPfizerBrandId, brandData.GlobalPfizerBrandName);
                                                    }
                                                    else if (globalValue != null)
                                                    {
                                                        if (!string.Equals(globalValue.TextValue, brandData.GlobalPfizerBrandName) && !processedValues.ContainsKey(brandData.GlobalPfizerBrandId))
                                                        {
                                                            globalValue.TextValue = brandData.GlobalPfizerBrandName;
                                                            valuesToUpdate.Add(globalValue);
                                                            processedValues.Add(brandData.GlobalPfizerBrandId, brandData.GlobalPfizerBrandName);
                                                        }
                                                    }

                                                    if (processedValues.ContainsKey(brandData.GlobalPfizerBrandId))
                                                    {
                                                        globalValue = valuesToInsert.SingleOrDefault(p => p.ExternalId == brandData.GlobalPfizerBrandId && !p.ParentId.HasValue)
                                                            ?? valuesToUpdate.SingleOrDefault(p => p.ExternalId == brandData.GlobalPfizerBrandId && !p.ParentId.HasValue);
                                                    }

                                                    if (localValue != null && (!string.Equals(localValue.TextValue, brandData.PfizerBrandName) || (globalValue != null && localValue.ParentId != globalValue.ObjectId)))
                                                    {
                                                        localValue.TextValue = brandData.PfizerBrandName;
                                                        if (globalValue != null && globalValue.ObjectId != Guid.Empty)
                                                        {
                                                            localValue.ParentId = globalValue.ObjectId;
                                                        }
                                                        else
                                                        {
                                                            if (globalValue.ChildValues == null)
                                                            {
                                                                globalValue.ChildValues = new List<ValueBO>();
                                                            }

                                                            globalValue.ChildValues.Add(localValue);
                                                        }
                                                        valuesToUpdate.Add(localValue);
                                                    }
                                                    else if (globalValue == null && localValue != null && localValue.ParentValue != null)
                                                    {
                                                        localValue.ParentId = null;
                                                        valuesToUpdate.Add(localValue);
                                                    }
                                                    else if (localValue == null)
                                                    {
                                                        localValue = new ValueBO()
                                                        {
                                                            TextValue = brandData.PfizerBrandName,
                                                            ExternalId = brandData.PfizerBrandId,
                                                            ValueListId = brandElement.ValueListId,
                                                            CountryId = countryId,
                                                            Status = (byte)ValueStatus.Active
                                                        };

                                                        if (Convert.ToInt32(brandData.GlobalPfizerBrandId) > 0)
                                                        {

                                                            if (valuesToInsert.Contains(globalValue))
                                                            {
                                                                if (globalValue.ChildValues == null)
                                                                {
                                                                    globalValue.ChildValues = new List<ValueBO>();
                                                                }

                                                                globalValue.ChildValues.Add(localValue);
                                                            }
                                                            else
                                                            {
                                                                localValue.ParentId = globalValue.ObjectId;
                                                                valuesToInsert.Add((localValue));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            valuesToInsert.Add(localValue);
                                                        }


                                                    }


                                                }


                                                ValueBO brandToDeactivate;
                                                List<Guid> tagsToRemove = new List<Guid>();
                                                //deactivate brand which are not in file and are still active
                                                foreach (ValueBO v in values.Where(o => o.Status == (byte)ValueStatus.Active))
                                                {

                                                    if (!brandDatas.Any(o => o.PfizerBrandName.ToLowerInvariant() == v.TextValue.ToLowerInvariant()))
                                                    {
                                                        v.Status = (byte)ValueStatus.Inactive;
                                                        valuesToUpdate.Add(v);
                                                        tagsToRemove.Add(v.ObjectId);
                                                        if (country.Code == "XX" && v.ParentId.HasValue)
                                                        {
                                                            brandToDeactivate = valueManager.GetObject(v.ParentId.Value);
                                                            brandToDeactivate.Status = (byte)ValueStatus.Inactive;
                                                            valuesToUpdate.Add(brandToDeactivate);
                                                            tagsToRemove.Add(brandToDeactivate.ObjectId);
                                                        }
                                                    }

                                                }




                                                using (TransactionScope transactionScope = TransactionScopeBuilder.CreateScope())
                                                {

                                                    IEnumerable<ValueBO> distinctValues = valuesToInsert.Distinct(new ValueComparer());
                                                    valueManager.InsertObjects(distinctValues, true);
                                                    valueManager.UpdateObjects(valuesToUpdate);

                                                    foreach (ValueBO value in valuesToInsert)
                                                    {
                                                        foreach (ValueBO valueDetail in (value.ChildValues ?? new List<ValueBO>()))
                                                        {
                                                            valueDetail.ParentId = value.ObjectId;
                                                            if (valueDetail.ObjectId == Guid.Empty)
                                                            {
                                                                valuesDetailsToInsert.Add(valueDetail);
                                                            }
                                                            else
                                                            {
                                                                valuesDetailsToUpdate.Add(valueDetail);
                                                            }
                                                        }
                                                    }

                                                    valueManager.InsertObjects(valuesDetailsToInsert);
                                                    valueManager.UpdateObjects(valuesDetailsToUpdate);

                                                    foreach (Guid tagValueId in tagsToRemove)
                                                    {
                                                        valueTagManager.RemoveTagsByValueId(tagValueId);
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
            }
        }

        public List<string> GetFileNamesToImport(int interval = 0)
        {
            List<string> result = new List<string>();

            using (BrandImportHistoryManager brandImportHistoryManager = new BrandImportHistoryManager())
            {
                DateTime currentDate = DateTime.Now.ToUniversalTime().Date.AddDays(interval);


                List<BrandImportHistoryBO> todayImports = brandImportHistoryManager.GetObjectsUsingBOPredicate(o => o.Date == currentDate && o.FileName.Contains("Brand"));

                using (AmazonS3Client client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, RegionEndpoint.GetBySystemName(configuration.Region)))
                {
                    S3DirectoryInfo directoryInfo = !string.IsNullOrEmpty(configuration.Directory) ? new S3DirectoryInfo(client, configuration.BucketName, configuration.Directory) : new S3DirectoryInfo(client, configuration.BucketName);
                    S3FileInfo[] fileInfos = directoryInfo.GetFiles(string.Format(searchPatternTemplate, currentDate.ToString("yyyyMMdd")), SearchOption.AllDirectories);

                    foreach (S3FileInfo fileInfo in fileInfos)
                    {
                        if (todayImports.All(o => o.FileName != fileInfo.Name))
                        {
                            result.Add(fileInfo.FullName.Substring(configuration.BucketName.Length + (!string.IsNullOrEmpty(configuration.Directory) ? configuration.Directory.Length + 1 : 0) + 2));
                        }
                    }
                }
            }

            if (result.Count > 1)
            {
                int index = result.FindIndex(o => o.StartsWith("XX_"));
                if (index >= 0 && index < result.Count - 1)
                {
                    string xxFileName = result[index];
                    result.RemoveAt(index);
                    result.Add(xxFileName);
                }
            }

            return result;
        }

        private List<BrandData> GetData(string key, out string fileName)
        {
            List<BrandData> result = new List<BrandData>();

            using (AmazonS3Client client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                S3DirectoryInfo directoryInfo = !string.IsNullOrEmpty(configuration.Directory) ? new S3DirectoryInfo(client, configuration.BucketName, configuration.Directory) : new S3DirectoryInfo(client, configuration.BucketName);
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

                                result = GetDataInternal(decompressedStream);
                            }
                        }
                    }
                    else
                    {
                        result = GetDataInternal(stream);
                    }
                }

                fileName = fileInfo.Name;
            }

            return result;
        }

        private List<BrandData> GetDataInternal(Stream stream)
        {
            List<BrandData> result = new List<BrandData>();

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
                            BrandData brandData = BrandData.Create(row.Values);

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

        class ValueComparer : IEqualityComparer<ValueBO>
        {
            public bool Equals(ValueBO x, ValueBO y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(ValueBO obj)
            {
                if (!string.IsNullOrEmpty(obj.TextValue) && obj.CountryId.HasValue)
                {
                    return (obj.TextValue + obj.CountryId.Value.ToString()).GetHashCode();
                }
                else
                {
                    return obj.TextValue.GetHashCode();
                }
            }
        }
    }
}
