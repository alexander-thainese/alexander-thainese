using CMT.BL.S3;
using CMT.BO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace CMTFileCopy
{
    public partial class CMTFileCopy : ServiceBase
    {
        private static CMTLogger logger = new CMTLogger();

        public CMTFileCopy()
        {
            InitializeComponent();
            //ProcessStartInfo start = new ProcessStartInfo();
            //start.FileName = @"C:\Program Files (x86)\Python 35\python.exe"; //
            //start.Arguments = "-m pip install boto3";
            //using (Process process = Process.Start(start)){}
        }

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = ApplicationSettings.CMTFileCopyInterval.TotalMilliseconds;

            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {

            logger.LogAction("Start processing", GetType());
            List<S3FileBO> files = S3Helper.GetFiles(ApplicationSettings.CMTFileCopySourceConfiguration);
            List<string> filteredFiles = files.Where(p => p.Date >= DateTime.UtcNow.Subtract(ApplicationSettings.CMTFileCopyMaxFileAgeForProcessing)).Select(p => p.FileName).ToList();

            foreach (string f in filteredFiles)
            {
                logger.LogAction("Processing file: " + f, GetType());
                RunPythonScript(f);
            }



        }

        protected override void OnStop()
        {

        }

        protected override void OnContinue()
        {

        }

        private void RunPythonScript(string filePath)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            CMT.Common.AWSConfigurationItem srcConfig = ApplicationSettings.CMTFileCopySourceConfiguration;
            CMT.Common.AWSConfigurationItem dstConfig = ApplicationSettings.CMTFileCopyDestConfiguration;
            filePath = srcConfig.Directory + "/" + filePath;
            start.FileName = "C:\\Program Files (x86)\\Python 3.5\\python.exe";
            string arguments = "C:\\inetpub\\Services\\CMTFileCopy\\S3FileSplitter.py --srcs3accesskey \"{0}\" --srcs3secretkey \"{1}\" --srcs3bucket \"{2}\" --srcs3filekey \"{3}\" --dsts3accesskey \"{4}\" --dsts3secretkey \"{5}\" --dsts3bucket \"{6}\" --dsts3filekey \"{7}\" -w \"{8}\" --srcgzip --sfs=\"|\"";
            start.Arguments = string.Format(arguments, srcConfig.AccessKey, srcConfig.SecretKey, srcConfig.BucketName, filePath,
                dstConfig.AccessKey, dstConfig.SecretKey, dstConfig.BucketName, dstConfig.Directory, @"C:\inetpub\Services\CMTFileCopy");
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    logger.LogAction("Python execution : " + start.Arguments, GetType());
                    string result = reader.ReadToEnd();
                    logger.LogAction("Python execution result : " + result, GetType());
                    //Console.Write(result);
                }
                using (StreamReader reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    logger.LogError(GetType(), new Exception(result));

                }
            }

        }
    }
}
