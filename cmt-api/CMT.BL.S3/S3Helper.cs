using Amazon.S3;
using Amazon.S3.Model;
using CMT.BO;
using CMT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMT.BL.S3
{
    public class S3Helper
    {
        private static readonly string bucketName = "candf.cmt";

        public static string awsAccessKey { get { throw new NotImplementedException(); } }
        public static string awsSecretKey { get { throw new NotImplementedException(); } }

        //private static readonly string _bucketName = ConfigurationManager.AppSettings["Bucketname"];
        static IAmazonS3 client;

        /// <summary>
        /// Downloads file from S3 Location
        /// </summary>
        /// <param name="configuration">S3 Configuration</param>
        /// <param name="fileFullName">Source file name</param>
        /// <param name="destinationFolder">Destination folder</param>
        /// <param name="uniqueFileName">Destination file name (without extension)</param>
        /// <returns>Destination unique file name</returns>
        public static string DownloadFile(AWSConfigurationItem configuration, string fileFullName, string destinationFolder, string uniqueFileName = "")
        {
            using (client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, Amazon.RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = configuration.BucketName,
                    Key = fileFullName
                };

                if (string.IsNullOrEmpty(uniqueFileName))
                {
                    uniqueFileName = Guid.NewGuid().ToString();
                }

                using (GetObjectResponse response = client.GetObject(request))
                {
                    string dest = Path.Combine(destinationFolder, uniqueFileName + Path.GetExtension(fileFullName));
                    if (!File.Exists(dest))
                    {
                        response.WriteResponseStreamToFile(dest);
                    }
                    return uniqueFileName;
                }
            }
        }

        public static string GetPath(params string[] parts)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string item in parts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append("/");
                }

                sb.Append(item.Trim('/'));
            }
            return sb.ToString();
        }

        public static bool FileExists(AWSConfigurationItem configuration, string fileKey)
        {
            using (client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, Amazon.RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                try
                {
                    GetObjectMetadataResponse metadata = client.GetObjectMetadata(configuration.BucketName, GetPath(configuration.Directory, fileKey));
                    return true;
                }
                catch (Amazon.S3.AmazonS3Exception ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false;
                    }

                    throw (ex);
                }
            }
        }

        // public static SendFile()
        public static bool SendFile(string fileName, string filePath)
        {
            using (client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.EUWest1))
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    FilePath = filePath,
                };

                PutObjectResponse response = client.PutObject(putRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static List<S3FileBO> GetFiles(AWSConfigurationItem configuration, string subDirectory = "")
        {
            using (client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, Amazon.RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                List<S3FileBO> result = new List<S3FileBO>();
                ListObjectsResponse response;
                string folder = configuration.Directory + (string.IsNullOrEmpty(subDirectory) ? "" : (configuration.Directory.EndsWith("/") || subDirectory.StartsWith("/") ? "" : "/") + subDirectory);
                do
                {
                    response = client.ListObjects(new ListObjectsRequest { BucketName = configuration.BucketName, Prefix = folder + (folder.EndsWith("/") ? "" : "/") });
                    result.AddRange(response.S3Objects.Select(p => new S3FileBO() { FullName = p.Key, Date = p.LastModified }).Where(p => !p.FullName.EndsWith("/")));
                }
                while (response.IsTruncated);
                return result;
            }
        }
        public static bool SendFileContent(string fileName, string content)
        {
            using (client = new AmazonS3Client(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.EUWest1))
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    ContentBody = content,

                };

                PutObjectResponse response = client.PutObject(putRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        public static bool SendFileStream(string fileName, Stream content, AWSConfigurationItem configuration)
        {
            using (client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, Amazon.RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = configuration.BucketName,
                    Key = string.Format("{0}{1}{2}", configuration.Directory, string.IsNullOrEmpty(configuration.Directory) ? "" : "/", fileName),
                    InputStream = content,
                };

                PutObjectResponse response = client.PutObject(putRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool SendFile(string fileName, string filePath, AWSConfigurationItem configuration)
        {
            using (client = new AmazonS3Client(configuration.AccessKey, configuration.SecretKey, Amazon.RegionEndpoint.GetBySystemName(configuration.Region)))
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = configuration.BucketName,
                    Key = string.Format("{0}{1}{2}", configuration.Directory, string.IsNullOrEmpty(configuration.Directory) ? "" : "/", fileName),
                    FilePath = filePath
                };

                PutObjectResponse response = client.PutObject(putRequest);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
