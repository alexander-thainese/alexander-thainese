//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.AccessControl;
//using System.Text;
//using System.Threading.Tasks;
//using CF.Common;
//using CMT.BL;
//using CMT.BL.DataMigrator;
//using CMT.BO;
//using CMT.BO.Import;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using CMT.DL.Core;

//namespace CMT.Core.UT
//{
//    [TestClass()]
//    public class FileImporterTests : BaseTest
//    {
//        private static DirectoryInfo directoryInfo;
//        private static bool deleteDirectory;
//        private static Guid importId;
//        private static string storageFileName;

//        public FileImporterTests()
//        {
//        }

//        [ClassInitialize()]
//        public static void FileImporter_ClassInit(TestContext context)
//        {
//            if (!Directory.Exists(ApplicationSettings.FileStorageFolder))
//            {
//                directoryInfo = Directory.CreateDirectory(ApplicationSettings.FileStorageFolder);
//                deleteDirectory = true;
//            }
//            else
//            {
//                directoryInfo = new DirectoryInfo(ApplicationSettings.FileStorageFolder);
//            }
//    }

//    [TestMethod]
//        public void FileImporter_CreateImportWithCampaignTest()
//        {
//            //FileImporter fileImporter = new FileImporter();
//            //importId = fileImporter.CreateImport(@"campaign_test.xlsx", new Guid("1834BC18-F3CF-4697-BCC2-3BBE5C6B9111"));
//            //Assert.IsTrue(importId != Guid.Empty);
//            Assert.IsTrue(false);
//        }

//        [TestMethod]
//        public void FileImporter_UploadFileTest()
//        {
//            Assert.IsTrue(importId != Guid.Empty, "UploadFileTest must run after CreateImportWithCampaignTest.");

//            FileImporter fileImporter = new FileImporter();
//            bool result = fileImporter.UploadFile(importId);
//            Assert.IsTrue(result);

//            using (ImportManager importManager = new ImportManager())
//            {
//                ImportBO import = importManager.GetObject(importId);
//                storageFileName = Path.Combine(ApplicationSettings.FileStorageFolder, string.Format("{0}{1}", importId, Path.GetExtension(import.FileName)));
//                Assert.IsTrue(File.Exists(storageFileName));
//            }
//        }

//        [TestMethod]
//        public void FileImporter_ImportFileTest()
//        {
//            Assert.IsTrue(importId != Guid.Empty, "ImportFileTest must run after CreateImportWithCampaignTest.");
//            Assert.IsTrue(!string.IsNullOrEmpty(storageFileName), "ImportFileTest must run after UploadFileTest.");

//            FileImporter fileImporter = new FileImporter();
//            fileImporter.ImportFile(importId);

//            using (SourceTableManager sourceTableManager = new SourceTableManager())
//            {
//                Assert.IsTrue(sourceTableManager.GetObjectsUsingBOPredicate(o => o.ImportId == importId).Any());
//            }
//        }

//        [ClassCleanup()]
//        public static void FileImporter_ClassCleanup()
//        {
//            if (directoryInfo != null && deleteDirectory)
//            {
//                directoryInfo.Delete(true);
//            }

//            if (!deleteDirectory && !string.IsNullOrEmpty(storageFileName))
//            {
//                File.Delete(storageFileName);
//            }

//            if (importId != Guid.Empty)
//            {
//                using (SourceTableManager sourceTableManager = new SourceTableManager())
//                {
//                    List<SourceTableBO> records = sourceTableManager.GetObjectsUsingBOPredicate(o => o.ImportId == importId);
//                    sourceTableManager.DeleteObjects(records);
//                }

//                using (ImportManager importManager = new ImportManager())
//                {
//                    importManager.DeleteObject(importId);
//                }
//            }
//        }
//    }
//}
