//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BL.Core;
//using CMT.BO;
//using CMT.Common;
//using FlexCel.XlsAdapter;
//using CMT.BL.S3;

//namespace CMT.BL.DataDistinctor
//{
//    public class FileCopyWorker : BaseWorker
//    {
//        private ISystemLogger logger;

//        public FileCopyWorker(ISystemLogger logger)
//            : base(logger)
//        {
//            this.logger = logger;
//        }

//        protected override void ProcessWork()
//        {
//            try
//            {
//                using (ImportManager importManager = new ImportManager())
//                {
//                    List<ImportBO> imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == ImportStatus.Created);

//                    foreach (ImportBO import in imports)
//                    {
//                        try
//                        {
//                            string fileName = string.Format("{0}{1}", import.FileUniqueName.ToString().ToLower(), Path.GetExtension(import.FileName));

//                            using (FileStream sourceFileStream = File.OpenRead(Path.Combine(ApplicationSettings.SourceFilesFolder, fileName)))
//                            {
//                                if (Path.GetExtension(fileName).ToLower() == FileType.Xls.ToString().ToLower())
//                                {
//                                    try
//                                    {
//                                        XlsFile xlsFile = new XlsFile(false);
//                                        xlsFile.Open(sourceFileStream);
//                                        sourceFileStream.Position = 0;
//                                    }
//                                    catch (Exception exc)
//                                    {
//                                        throw new FileFormatNotSupportedException("Cannot load data from file. File format not supported.", exc);
//                                    }
//                                }

//                                string storageFileName = Path.Combine(ApplicationSettings.FileStorageFolder, fileName);

//                                using (FileStream destinationFileStream = new FileStream(storageFileName, FileMode.CreateNew, FileAccess.Write))
//                                {
//                                    sourceFileStream.CopyTo(destinationFileStream);
//                                }


//                                var configuration = ApplicationSettings.ProcessedFileArchiveConfiguration;
//                                try
//                                {
//                                    var storagePath = Path.Combine(ApplicationSettings.FileStorageFolder, import.FileUniqueName + Path.GetExtension(import.FileName));
//                                    var sourcePath = Path.Combine(ApplicationSettings.SourceFilesFolder, import.FileUniqueName + Path.GetExtension(import.FileName));

//                                    if (File.Exists(storagePath) || File.Exists(sourcePath))
//                                    {
//                                        var filePath = File.Exists(storagePath) ? storagePath : sourcePath;

//                                        var existsAtS3 = S3Helper.FileExists(configuration, import.FileUniqueName + Path.GetExtension(import.FileName));
//                                        bool result = false;
//                                        if (!existsAtS3)
//                                            result = S3Helper.SendFile(import.FileUniqueName + Path.GetExtension(import.FileName), filePath, configuration);


//                                    }
//                                }
//                                catch (IOException) { }


//                                import.Status = ImportStatus.FileReady;
//                                importManager.UpdateObject(import);
//                            }
//                        }
//                        catch (Exception exc)
//                        {
//                            import.Status = ImportStatus.Failed;

//                            if (exc is FileFormatNotSupportedException)
//                            {
//                                import.Message = exc.Message;
//                            }
//                            else
//                            {
//                                import.Message = "Unable to copy file to working directory";
//                            }

//                            importManager.UpdateObject(import);
//                            throw new Exception(string.Format("Error while copying file for import with ObjectId = '{0}'.", import.ObjectId), exc);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
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
//            return ApplicationSettings.DistinctCheckInterval;
//        }
//    }

//    public class FileFormatNotSupportedException : Exception
//    {
//        public FileFormatNotSupportedException(string message, Exception innerException) 
//            : base(message, innerException)
//        {
//        }
//    }
//}
