using CMT.Common;
using CMT.Core.UT;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMT.BL.DataDistinctor.Tests
{
    [TestClass()]
    public class DataDistinctorTests : BaseTest
    {
        public DataDistinctorTests()
        {
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
        }

        [TestMethod()]
        public void ProductData_ShouldCreateCorrectObject_WithGivenConfig()
        {
            ProductPackDataRowConfig rowConfig1 = new ProductPackDataRowConfig(Guid.NewGuid(), 4);
            ProductPackDataRowConfig rowConfig2 = new ProductPackDataRowConfig(Guid.NewGuid(), 5);
            ProductPackDataConfig config = new ProductPackDataConfig()
            {
                RowConfigs = new List<ProductPackDataRowConfig>()
                {
                    rowConfig1,
                    rowConfig2
                }
            };
            object[] row = new object[20];
            for (int i = 0; i < row.Length; i++) row[i] = $"data{i}";

            ProductPackData data = ProductPackData.Create(row, config);
            data.CustomAttributes.Should().NotBeNullOrEmpty();
            data.CustomAttributes.Count.Should().Be(config.RowConfigs.Count);
            data.GlobalCode.Should().Be((string)row[config.GlobalCodeColumnIndex]);
            data.GlobalPfizerBrandId.Should().Be((string)row[config.GlobalPfizerBrandIdColumnIndex]);
            data.GlobalPfizerBrandName.Should().Be((string)row[config.GlobalPfizerBrandNameColumnIndex]);

            data.CustomAttributes.Single(o => o.MetaDataElementId == rowConfig1.MetaDataElementId)
                .Value.Should().Be((string)row[rowConfig1.ColumnIndex]);
            data.CustomAttributes.Single(o => o.MetaDataElementId == rowConfig2.MetaDataElementId)
                .Value.Should().Be((string)row[rowConfig2.ColumnIndex]);

        }

        [TestMethod()]
        public void ProductData_ShouldNotCreateObject_IfGlobalPfizerBrandIdValueIsNull()
        {
            ProductPackDataRowConfig rowConfig1 = new ProductPackDataRowConfig(Guid.NewGuid(), 4);
            ProductPackDataRowConfig rowConfig2 = new ProductPackDataRowConfig(Guid.NewGuid(), 5);
            ProductPackDataConfig config = new ProductPackDataConfig()
            {
                RowConfigs = new List<ProductPackDataRowConfig>()
                {
                    rowConfig1,
                    rowConfig2
                }
            };
            object[] row = new object[20];
            for (int i = 0; i < row.Length; i++) row[i] = $"data{i}";
            row[config.GlobalPfizerBrandIdColumnIndex] = null;

            ProductPackData data = ProductPackData.Create(row, config);
            data.Should().Be(null);

        }

        [TestMethod()]
        public void CMTConfig_ShouldCreateCorrectObject_JsonString()
        {
            string configJson = "{\"AWSConfigurationItems\":[{\"Name\":\"AttributeUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\CF\"},{\"Name\":\"BrandUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\Brands\"},{\"Name\":\"ExtractsUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\Extracts\"}],\"Settings\":{\"FileStorageFolder\":\"D:\\\\tmp\\\\CMT\\\\\",\"SourceFilesFolder\":\"D:\\\\tmp\\\\CMT\\\\Source\",\"ReturnErrorMessage\":\"True\",\"FileUploadCheckInterval\":\"0:0:10\",\"DistinctCheckInterval\":\"0:0:10\",\"UseNewDistinctor\":\"true\",\"DocumentationVersion\":\"1.0.0\",\"FileExtractorInterval\":\"00:00:01\",\"LoadExtractsToS3\":\"false\",\"LoadExtractsToLocationPath\":\"true\",\"FileExtractorLocationPath\":\"C:\\\\tmp\\\\\",\"BrandUploadInterval\":\"1:0:1\",\"AttributeUploadInterval\":\"1:0:1\",\"RdmApplicationIPs\":\"10.46.1.116\",\"DisableWorkers\":\"true\",\"DisableArchiving\":\"true\",\"ArchiveDelayOffset\":\"0:0:15:0\",\"CMTFileCopyInterval\":\"30:0:0:0\",\"CMTFileCopyMaxFileAgeForProcessing\":\"30:0:0:0\",\"CommandTimeout\":\"900\",\"CorsUrls\":\"http://localhost:4200,http://metadata-dev-app.s3-website-eu-west-1.amazonaws.com,http://cmt-dev.pfizer.com\"},\"ProductPackConfig\":{\"RowConfigs\":[{\"MetaDataElementId\":\"4023aa1f-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":16},{\"MetaDataElementId\":\"60e2ff26-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":17},{\"MetaDataElementId\":\"0861a1d1-4419-e711-97ba-0296c03ebb49\",\"ColumnIndex\":18},{\"MetaDataElementId\":\"b4d2132d-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":15},{\"MetaDataElementId\":\"809e2a04-c29b-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":14}],\"GlobalPfizerBrandIdColumnIndex\":12,\"GlobalPfizerBrandNameColumnIndex\":13,\"GlobalCodeColumnIndex\":19,\"SearchPatternTemplate\":\"XX*_ProductPack_{0}_*.*\"}}";
            CMTConfig c = JsonConvert.DeserializeObject<CMTConfig>(configJson);
            c.Should().NotBeNull();
            c.AWSConfigurationItems.Count.Should().Be(3);
            c.Settings.Count.Should().Be(21);
            c.ProductPackConfig.RowConfigs.Count.Should().Be(5);
            c.AWSConfigurationItems[0].Name.Should().Be("AttributeUploader");
            c.Settings["DisableWorkers"].Should().Be("true");
            c.ProductPackConfig.GlobalCodeColumnIndex.Should().Be(19);
            c.ProductPackConfig.SearchPatternTemplate.Should().Be("XX*_ProductPack_{0}_*.*");

        }
    }
}