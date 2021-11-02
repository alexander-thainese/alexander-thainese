//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BL.Core;
//using CMT.BO;
//using CMT.Common;
//using CMT.DataDistinctor;
//using DDR.Common;
//using DDR.DataStore;
//using DDR.Files.Tabular.Structure;
//using DDR.Locations;
//using CMT.BL.S3;

//namespace CMT.BL.DataDistinctor
//{
//    public class StructureDiscoveryWorker : BaseWorker
//    {
//        private int cacheSize = ApplicationSettings.LoadCacheSize;
//        private SHA1 sha1;
//        private ISystemLogger logger;

//        public StructureDiscoveryWorker(ISystemLogger logger)
//            : base(logger)
//        {
//            this.sha1 = SHA1.Create();
//            this.logger = logger;
//        }

//        protected override void ProcessWork()
//        {
//            try
//            {
//                using (ImportManager importManager = new ImportManager())
//                {
//                    List<ImportBO> imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == ImportStatus.FileReady);

//                    foreach (ImportBO import in imports)
//                    {
//                        try
//                        {
//                            using (CmtFileSystemTempDirectory directory = new CmtFileSystemTempDirectory(ApplicationSettings.FileStorageFolder))
//                            {


//                                var fileName = import.FileUniqueName + Path.GetExtension(import.FileName);
//                                var filePath = Path.Combine(ApplicationSettings.FileStorageFolder, fileName);
//                                if (!File.Exists(filePath))
//                                {
//                                    var configuration = ApplicationSettings.ProcessedFileArchiveConfiguration;
//                                    S3Helper.DownloadFile(configuration, S3Helper.GetPath(configuration.Directory, import.FileUniqueName + Path.GetExtension(import.FileName)), ApplicationSettings.FileStorageFolder, import.FileUniqueName.ToString());
//                                }



//                                Location sourceLocation = new FileSystemLocation(Path.Combine(ApplicationSettings.FileStorageFolder, Path.ChangeExtension(import.FileUniqueName.ToString().ToLower(), Path.GetExtension(import.FileName))));
//                                DataDistinctorService service = new DataDistinctorService(directory, GetFileStore);
//                                TableInfo table = service.DiscoverStructure(sourceLocation, this.GetDataFormat(import));

//                                using (DistinctRowManager distinctRowManager = new DistinctRowManager())
//                                {
//                                    distinctRowManager.BeginBulkUpload();
//                                    DataTable dataTable = distinctRowManager.CreateEmptyDataTable();
//                                    bool recordsLeft = true;
//                                    int startIndex = 0;
//                                    int processedRecords = 0;

//                                    do
//                                    {
//                                        for (int i = startIndex; i < this.cacheSize; i++)
//                                        {
//                                            if (table.Columns.Count <= i)
//                                            {
//                                                recordsLeft = false;
//                                                break;
//                                            }

//                                            DataRow tableRow = dataTable.NewRow();
//                                            tableRow["ObjectId"] = this.GenerateObjectId(import.ObjectId, table.Columns[i].Name, table.Columns[i].Name);
//                                            tableRow["ColumnName"] = table.Columns[i].Name;
//                                            tableRow["Value"] = table.Columns[i].Name;
//                                            tableRow["IsHeader"] = true;
//                                            tableRow["ImportId"] = import.ObjectId;
//                                            tableRow["LineNumber"] = 0;
//                                            tableRow["ColumnIndex"] = table.Columns[i].Index;
//                                            dataTable.Rows.Add(tableRow);

//                                            processedRecords = i;
//                                        }

//                                        startIndex = processedRecords + 1;

//                                        distinctRowManager.UploadRecords(dataTable);
//                                        dataTable.Rows.Clear();
//                                    } while (recordsLeft);

//                                    distinctRowManager.EndBulkUpload();
//                                }
//                            }

//                            import.Status = ImportStatus.ColumnRead;
//                            importManager.UpdateObject(import);
//                        }
//                        catch (Exception exc)
//                        {
//                            import.Status = ImportStatus.Failed;
//                            import.Message = string.Format("Column extraction failed. {0}", exc.Message);
//                            importManager.UpdateObject(import);
//                            throw new Exception(string.Format("Column extraction failed for import with ObjectId = '{0}'.", import.ObjectId), exc);
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

//        private IFileStore GetFileStore(Location location)
//        {
//            return new LocalFileSystemFileStore();
//        }

//        private DataFormat GetDataFormat(ImportBO import)
//        {
//            switch (import.FileType)
//            {
//                case FileType.Txt:
//                    return DataFormat.Txt;
//                case FileType.Csv:
//                    return DataFormat.Csv;
//                case FileType.Xls:
//                    return DataFormat.Xls;
//                case FileType.Xlsx:
//                    return DataFormat.Xlsx;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        private Guid GenerateObjectId(Guid importId, string columnName, string value)
//        {
//            string text = string.Format("{0}{1}{2}", importId.ToString().ToLower(), columnName, value);
//            byte[] hash = this.sha1.ComputeHash(Encoding.Default.GetBytes(text));
//            return new Guid(hash.Take(16).ToArray());
//        }
//    }
//}
