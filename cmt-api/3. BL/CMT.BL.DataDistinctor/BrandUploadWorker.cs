using CMT.BL.Core;
using CMT.BO;
using System;
using System.Collections.Generic;

namespace CMT.BL.DataDistinctor
{
    public class BrandUploadWorker : BaseWorker
    {
        private ISystemLogger logger;

        public BrandUploadWorker(ISystemLogger logger)
            : base(logger)
        {

            this.logger = logger;

        }

        protected override void ProcessWork()
        {
            try
            {

                BrandUploader brandUploader = new BrandUploader();
                List<string> fileKeyToUpload = brandUploader.GetFileNamesToImport();

                logger.LogAction("BrandUploadWorker start processing. Files to process " + fileKeyToUpload.Count, GetType());
                foreach (string fileKey in fileKeyToUpload)
                {

                    try
                    {
                        logger.LogAction("BrandUploadWorker start processing. File: " + fileKey, GetType());
                        brandUploader.ImportFile(fileKey);
                    }
                    catch (Exception fileException)
                    {
                        logger.LogAction("BrandUploadWorker error. File: " + fileKey, GetType());
                        logger.LogError(GetType(), fileException);
                    }
                    

                        
                    


                }
                logger.LogAction("BrandUploadWorker end processing.", GetType());
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(GetType(), ex);
                }
            }
        }

        protected override TimeSpan GetTimeDelayToStartWork()
        {
            return GetThreadLoopDelay();
        }

        protected override TimeSpan GetThreadLoopDelay()
        {
            return ApplicationSettings.BrandUploadInterval;
        }
    }
}
