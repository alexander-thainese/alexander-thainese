using CF.Common;
using CMT.BL;
using CMT.BO;
using CMT.Common;
using CMT.Controllers;
using CMT.DL;
using CMT.DL.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace CMT.Core.UT
{
    public class BaseTest
    {
        protected static Container Container { get; set; }
        [TestInitialize]
        public virtual void Setup()
        {
            //TODO get from db
            string configJson = "{\"AWSConfigurationItems\":[{\"Name\":\"AttributeUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\CF\"},{\"Name\":\"BrandUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\Brands\"},{\"Name\":\"ExtractsUploader\",\"AccessKey\":\"key\",\"SecretKey\":\"secret\",\"Region\":\"eu-west-1\",\"BucketName\":\"pfe-baiaes-eu-w1-nprod-project\",\"Directory\":\"cmt\\\\beanstalk\\\\Files\\\\Extracts\"}],\"Settings\":{\"FileStorageFolder\":\"D:\\\\tmp\\\\CMT\\\\\",\"SourceFilesFolder\":\"D:\\\\tmp\\\\CMT\\\\Source\",\"ReturnErrorMessage\":\"True\",\"FileUploadCheckInterval\":\"0:0:10\",\"DistinctCheckInterval\":\"0:0:10\",\"UseNewDistinctor\":\"true\",\"DocumentationVersion\":\"1.0.0\",\"FileExtractorInterval\":\"00:00:01\",\"LoadExtractsToS3\":\"false\",\"LoadExtractsToLocationPath\":\"true\",\"FileExtractorLocationPath\":\"C:\\\\tmp\\\\\",\"BrandUploadInterval\":\"1:0:1\",\"AttributeUploadInterval\":\"1:0:1\",\"RdmApplicationIPs\":\"10.46.1.116\",\"DisableWorkers\":\"true\",\"DisableArchiving\":\"true\",\"ArchiveDelayOffset\":\"0:0:15:0\",\"CMTFileCopyInterval\":\"30:0:0:0\",\"CMTFileCopyMaxFileAgeForProcessing\":\"30:0:0:0\",\"CommandTimeout\":\"900\",\"CorsUrls\":\"http://localhost:4200,http://metadata-dev-app.s3-website-eu-west-1.amazonaws.com,http://cmt-dev.pfizer.com\"},\"ProductPackConfig\":{\"RowConfigs\":[{\"MetaDataElementId\":\"4023aa1f-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":16},{\"MetaDataElementId\":\"60e2ff26-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":17},{\"MetaDataElementId\":\"0861a1d1-4419-e711-97ba-0296c03ebb49\",\"ColumnIndex\":18},{\"MetaDataElementId\":\"b4d2132d-4e99-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":15},{\"MetaDataElementId\":\"809e2a04-c29b-e811-b7c1-02dfca91e9e8\",\"ColumnIndex\":14}],\"GlobalPfizerBrandIdColumnIndex\":12,\"GlobalPfizerBrandNameColumnIndex\":13,\"GlobalCodeColumnIndex\":19,\"SearchPatternTemplate\":\"XX*_ProductPack_{0}_*.*\"}}";
            CMTConfig c = JsonConvert.DeserializeObject<CMTConfig>(configJson);
            ApplicationSettings.CMTConfig = c;
            AuditDataHelper.CollectAuditData += AuditDataHelper_CollectAuditData;
            ScopeDataStore.Register(new ThreadLocalScopeDataStore(), false);
            //new DbContextScope<CmtEntities>();
            DbConnectionFactory.Register(new DbConnectionFactory(), false);
            AddSimpleInjectorConfig();
        }

        [TestCleanup]
        public virtual void CleanUp()
        {
            SimpleInjectorConfig.DisposeContainer();
            SimpleInjectorConfig.Configure -= SimpleInjectorConfig_Configure;
        }

        public virtual void AddSimpleInjectorConfig()
        {
            SimpleInjectorConfig.Configure += SimpleInjectorConfig_Configure;
        }

        protected static void SimpleInjectorConfig_Configure(object sender, Common.ValueRequestEventArgs<Container> e)
        {
            Container = new Container();
            Container.Options.AllowOverridingRegistrations = true;
            Container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            Container.Register(() => new CmtEntities(), Lifestyle.Singleton);
            Container.Register<AdminMetadataController, AdminMetadataController>(Lifestyle.Singleton);
            Container.Register<ElementManager, ElementManager>(Lifestyle.Singleton);
            Container.Register<ElementTypeManager, ElementTypeManager>(Lifestyle.Singleton);
            Container.Register<ValueManager, ValueManager>(Lifestyle.Singleton);
            Container.Register<ValueDetailManager, ValueDetailManager>(Lifestyle.Singleton);
            Container.Register<ValueListManager, ValueListManager>(Lifestyle.Singleton);
            Container.Register<ValueTagManager, ValueTagManager>(Lifestyle.Singleton);
            Container.Register<ValueListLevelManager, ValueListLevelManager>(Lifestyle.Singleton);

            e.Value = Container;
        }
        private static void AuditDataHelper_CollectAuditData(object sender, AuditDataEventArgs e)
        {
            e.User = "TestProcess";
        }
    }
}
