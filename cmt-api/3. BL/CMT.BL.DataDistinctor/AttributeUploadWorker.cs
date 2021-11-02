using CMT.BL.Core;
using CMT.BO;
using System;
using System.Collections.Generic;

namespace CMT.BL.DataDistinctor
{
    public class AttributeUploadWorker : BaseWorker
    {
        private ISystemLogger logger;

        public AttributeUploadWorker(ISystemLogger logger)
            : base(logger)
        {

            this.logger = logger;
        }

        protected override void ProcessWork()
        {
           
               
                AttributeUploader uploader = new AttributeUploader();
                List<string> fileKeyToUpload = uploader.GetFileNamesToImport();
                logger.LogAction("AttributeUploadWorker start processing. Files to process " + fileKeyToUpload.Count, GetType());
                foreach (string fileKey in fileKeyToUpload)
                {

                    try
                    {

                        logger.LogAction("AttributeUploader start processing. File: " + fileKey, GetType());
                        uploader.ImportFile(fileKey);
                    }
                    catch (Exception ex)
                    {
                        if (logger != null)
                        {
                            logger.LogAction("AttributeUploader error during processing. File: " + fileKey, GetType());
                            logger.LogError(GetType(), ex);
                        }
                    }
                }
                logger.LogAction("AttributeUploadWorker end processing.", GetType());
            
           
        }

        protected override TimeSpan GetTimeDelayToStartWork()
        {
            return GetThreadLoopDelay();
        }

        protected override TimeSpan GetThreadLoopDelay()
        {
            return ApplicationSettings.AttributeUploadInterval;
        }
    }
}
