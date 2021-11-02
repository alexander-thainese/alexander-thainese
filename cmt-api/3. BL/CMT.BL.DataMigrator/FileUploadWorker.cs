//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BL.Core;
//using CMT.BO;

//namespace CMT.BL.DataMigrator
//{
//    public class FileUploadWorker : BaseWorker
//    {
//        private ISystemLogger logger;

//        public FileUploadWorker(ISystemLogger logger)
//            : base(logger)
//        {
//            this.logger = logger;
//        }

//        protected override void ProcessWork()
//        {
//            try
//            {
//                List<ImportBO> imports;

//                using (ImportManager importManager = new ImportManager())
//                {
//                    imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == Common.ImportStatus.Created);
//                }
//                foreach (ImportBO import in imports)
//                {
//                    FileReadSettings fileReadSettings = new FileReadSettings(import);
//                    FileImporter fileImporter = new FileImporter(fileReadSettings);

//                    if (fileImporter.UploadFile(import))
//                    {
//                        fileImporter.ImportFile(import);
//                    }
//                }
//            }
//            catch(Exception ex)
//            {
//                if (logger != null)
//                    logger.LogError(this.GetType(), ex);
//            }
//        }

//        protected override TimeSpan GetTimeDelayToStartWork()
//        {
//            return this.GetThreadLoopDelay();
//        }

//        protected override TimeSpan GetThreadLoopDelay()
//        {
//            return ApplicationSettings.FileUploadCheckInterval;
//        }
//    }


//}
