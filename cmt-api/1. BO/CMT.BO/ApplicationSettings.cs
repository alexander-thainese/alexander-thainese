using CMT.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CMT.BO
{
    public static class ApplicationSettings
    {
        public static CMTConfig CMTConfig { get; set; }

        public const int LoadCacheSize = 500;
        public const int ExecutionTimeout = 500;

        public static string FileStorageFolder
        {
            get { return CMTConfig.Settings["FileStorageFolder"]; }
        }

        public static string SourceFilesFolder
        {
            get { return CMTConfig.Settings["SourceFilesFolder"]; }
        }

        public static bool ReturnErrorMessage
        {
            get { return Convert.ToBoolean(CMTConfig.Settings["ReturnErrorMessage"]); }
        }

        public static TimeSpan FileUploadCheckInterval
        {
            get
            {
                string[] values = CMTConfig.Settings["FileUploadCheckInterval"].Split(':');
                if (values.Length != 3)
                {
                    throw new Exception("Invalid value of parameter 'FileUploadCheckInterval'");
                }

                return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
            }
        }

        public static TimeSpan DistinctCheckInterval
        {
            get
            {
                string[] values = CMTConfig.Settings["DistinctCheckInterval"].Split(':');
                if (values.Length != 3)
                {
                    throw new Exception("Invalid value of parameter 'DistinctCheckInterval'");
                }

                return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
            }
        }

        private static TimeSpan ParseTimeSpan(string key)
        {
            string[] values = CMTConfig.Settings[key].Split(':');
            switch (values.Length)
            {
                case 1:
                    //ticks
                    return new TimeSpan(long.Parse(values[0]));
                case 3:
                    //h:M:s
                    return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
                case 4:
                    //d:h:M:s
                    return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
                case 5:
                    //d:h:M:s:ms
                    return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]), int.Parse(values[4]));
                default:
                    throw new Exception(string.Format("{0} Application Setting - invalid format", key));
            }
        }

        public static TimeSpan FileExtractorInterval
        {
            get
            {
                return ParseTimeSpan("FileExtractorInterval");
            }
        }

        public static int CommandTimeout
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["CommandTimeout"]);
            }
        }
        public static TimeSpan ArchiveDelayOffset
        {
            get
            {
                return ParseTimeSpan("ArchiveDelayOffset");
            }
        }

        public static TimeSpan CMTFileCopyInterval
        {
            get { return ParseTimeSpan("CMTFileCopyInterval"); }
        }

        public static TimeSpan CMTFileCopyMaxFileAgeForProcessing { get { return ParseTimeSpan("CMTFileCopyMaxFileAgeForProcessing"); } }

        public static bool LoadExtractsToS3
        {
            get { return Convert.ToBoolean(CMTConfig.Settings["LoadExtractsToS3"]); }
        }

        public static bool LoadExtractsToLocationPath
        {
            get
            {
                return Convert.ToBoolean(CMTConfig.Settings["LoadExtractsToLocationPath"]);
            }
        }

        public static string FileExtractorLocationPath
        {
            get => CMTConfig.Settings["FileExtractorLocationPath"];
        }

        public static TimeSpan BrandUploadInterval
        {
            get
            {
                string[] values = CMTConfig.Settings["BrandUploadInterval"].Split(':');
                if (values.Length != 3)
                {
                    throw new Exception("Invalid value of parameter 'BrandUploadInterval'");
                }
                return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
            }
        }

        public static TimeSpan AttributeUploadInterval
        {
            get
            {
                string[] values = CMTConfig.Settings["AttributeUploadInterval"].Split(':');
                if (values.Length != 3)
                {
                    throw new Exception("Invalid value of parameter 'AttributeUploadInterval'");
                }

                return new TimeSpan(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
            }
        }

        public static string DocumentationVersion
        {
            get { return CMTConfig.Settings["DocumentationVersion"]; }
        }

        public static bool UseNewDistinctor
        {
            get { return Convert.ToBoolean(CMTConfig.Settings["UseNewDistinctor"]); }
        }

        public static string CorsUrls
        {
            get { return CMTConfig.Settings["CorsUrls"]; }
        }

        public static AWSConfigurationItem BrandUploaderConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "BrandUploader");
            }
        }
        public static AWSConfigurationItem AttributeUploaderConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "AttributeUploader");
            }
        }

        public static AWSConfigurationItem ExtractsUploaderConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "ExtractsUploader");
            }
        }

        public static AWSConfigurationItem ProcessedFileArchiveConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "ProcessedFileArchive");
            }
        }

        public static AWSConfigurationItem S3ImportFileSourceConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "S3ImportFileSource");
            }
        }

        public static AWSConfigurationItem CMTFileCopySourceConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "CMTFileCopySource");
            }
        }

        public static AWSConfigurationItem CMTFileCopyDestConfiguration
        {
            get
            {
                return CMTConfig.AWSConfigurationItems.Single(o => o.Name == "CMTFileCopyDest");
            }
        }

        public static List<string> RdmApplicationIPs
        {
            get { return CMTConfig.Settings["RdmApplicationIPs"].Split('|').ToList(); }
        }

        public static bool DisableWorkers
        {
            get
            {
                return Convert.ToBoolean(CMTConfig.Settings["DisableWorkers"]);
            }
        }

        public static bool DisableArchiving
        {
            get
            {
                return Convert.ToBoolean(CMTConfig.Settings["DisableArchiving"]);
            }
        }
    }
}