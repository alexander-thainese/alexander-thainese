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
//    public class DataDistinctorWorker : BaseWorker
//    {
//        private int cacheSize = ApplicationSettings.LoadCacheSize;

//        private ISystemLogger logger;

//        public DataDistinctorWorker(ISystemLogger logger)
//            : base(logger)
//        {

//            this.logger = logger;
//        }

//        protected override void ProcessWork()
//        {
//            try
//            {
//                Archiving();
//                this.InitialProcessing();
//                this.DistinctProcessing();


//            }
//            catch (Exception ex)
//            {
//                if (logger != null)
//                    logger.LogError(this.GetType(), ex);
//            }
//        }

//        private void Archiving()
//        {
//            if (ApplicationSettings.DisableArchiving)
//            {
//                return;
//            }
//            List<ImportBO> imports;
//            using (ImportManager importManager = new ImportManager())
//            {
//                var borderDate = DateTime.UtcNow.Subtract(ApplicationSettings.ArchiveDelayOffset);
//                imports = importManager.GetObjects(t => t.Status == (byte) ImportStatus.DistinctProcessingCompleted && (t.ChangeDate.HasValue ? t.ChangeDate.Value < borderDate : false));
//            }
//            foreach (var import in imports)
//            {
//                var configuration = ApplicationSettings.ProcessedFileArchiveConfiguration;
//                try
//                {
//                    var storagePath = Path.Combine(ApplicationSettings.FileStorageFolder, import.FileUniqueName + Path.GetExtension(import.FileName));
//                    var sourcePath = Path.Combine(ApplicationSettings.SourceFilesFolder, import.FileUniqueName + Path.GetExtension(import.FileName));

//                    if (File.Exists(storagePath) || File.Exists(sourcePath))
//                    {
//                        var filePath = File.Exists(storagePath) ? storagePath : sourcePath;

//                        var existsAtS3 = S3Helper.FileExists(configuration, import.FileUniqueName + Path.GetExtension(import.FileName));
//                        bool result = false;
//                        if (!existsAtS3)
//                            result = S3Helper.SendFile(import.FileUniqueName + Path.GetExtension(import.FileName), filePath, configuration);
//                        if (existsAtS3 || result)
//                        {
//                            if (File.Exists(storagePath))
//                                File.Delete(storagePath);
//                            if (File.Exists(sourcePath))
//                                File.Delete(sourcePath);
//                        }

//                    }
//                }
//                catch (IOException) { }
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

//        private void InitialProcessing()
//        {
//            using (ImportManager importManager = new ImportManager())
//            {
//                List<ImportBO> imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == ImportStatus.ColumnRead);

//                foreach (ImportBO import in imports)
//                {
//                    try
//                    {
//                        using (CmtFileSystemTempDirectory directory = new CmtFileSystemTempDirectory(ApplicationSettings.FileStorageFolder))
//                        {
//                            import.Status = ImportStatus.InitialProcessingStarted;
//                            importManager.UpdateObject(import);


//                            var fileName = import.FileUniqueName + Path.GetExtension(import.FileName);
//                            var filePath = Path.Combine(ApplicationSettings.FileStorageFolder, fileName);
//                            if (!File.Exists(filePath))
//                            {
//                                var configuration = ApplicationSettings.ProcessedFileArchiveConfiguration;
//                                S3Helper.DownloadFile(configuration, S3Helper.GetPath(configuration.Directory, import.FileUniqueName + Path.GetExtension(import.FileName)), ApplicationSettings.FileStorageFolder, import.FileUniqueName.ToString());
//                            }


//                            Location sourceLocation = new FileSystemLocation(Path.Combine(ApplicationSettings.FileStorageFolder, Path.ChangeExtension(import.FileUniqueName.ToString().ToLower(), Path.GetExtension(import.FileName))));
//                            DataDistinctorService service = new DataDistinctorService(directory, GetFileStore, false);

//                            using (DistinctRowManager distinctRowManager = new DistinctRowManager())
//                            {
//                                Guid importId = import.ObjectId;
//                                List<DistinctRowBO> distinctRows = distinctRowManager.GetObjectsUsingBOPredicate(o => o.ImportId == importId && o.IsHeader);
//                                int[] columnsToAnalyze = distinctRows.Select(o => o.ColumnIndex).ToArray();
//                                Dictionary<int, string> columnNames = distinctRows.ToDictionary(o => o.ColumnIndex, o => o.ColumnName);
//                                Dictionary<int, GroupedDistinctValues> distinctValues = service.GetDistinctValues(sourceLocation, this.GetDataFormat(import), new int[] { }, columnsToAnalyze);

//                                distinctRowManager.BeginBulkUpload();
//                                DataTable dataTable = distinctRowManager.CreateEmptyDataTable();
//                                bool recordsLeft = true;
//                                int startIndex = 0;
//                                int processedRecords = 0;

//                                List<DistinctRowBO> rowsToInsert = new List<DistinctRowBO>();

//                                foreach (int key in distinctValues.Keys)
//                                {
//                                    foreach (HashSet<object> set in distinctValues[key].DistinctValues.Values)
//                                    {
//                                        foreach (object value in set)
//                                        {
//                                            if (value != null)
//                                            {
//                                                rowsToInsert.Add(new DistinctRowBO()
//                                                {
//                                                    ObjectId = this.GenerateObjectId(import.ObjectId, columnNames[key], value.ToString()),
//                                                    ColumnName = columnNames[key],
//                                                    ColumnIndex = key,
//                                                    Value = value.ToString(),
//                                                    IsHeader = false,
//                                                    LineNumber = 0,
//                                                    ImportId = import.ObjectId
//                                                });
//                                            }
//                                        }
//                                    }
//                                }

//                                do
//                                {
//                                    for (int i = startIndex; i < processedRecords + this.cacheSize; i++)
//                                    {
//                                        if (rowsToInsert.Count <= i)
//                                        {
//                                            recordsLeft = false;
//                                            break;
//                                        }

//                                        DataRow tableRow = dataTable.NewRow();
//                                        tableRow["ObjectId"] = rowsToInsert[i].ObjectId;
//                                        tableRow["ColumnName"] = rowsToInsert[i].ColumnName;
//                                        tableRow["Value"] = rowsToInsert[i].Value;
//                                        tableRow["IsHeader"] = rowsToInsert[i].IsHeader;
//                                        tableRow["ImportId"] = rowsToInsert[i].ImportId;
//                                        tableRow["LineNumber"] = rowsToInsert[i].LineNumber;
//                                        tableRow["ColumnIndex"] = rowsToInsert[i].ColumnIndex;
//                                        dataTable.Rows.Add(tableRow);

//                                        processedRecords = i;
//                                    }

//                                    startIndex = processedRecords + 1;

//                                    distinctRowManager.UploadRecords(dataTable);
//                                    dataTable.Rows.Clear();
//                                } while (recordsLeft);

//                                distinctRowManager.EndBulkUpload();
//                                distinctRowManager.DbContext.Database.CommandTimeout = 1200;
//                                logger.LogAction("CreateRowValueImport start", this.GetType());

//                                distinctRowManager.DbContext.CreateRowValueImport(importId);
//                                logger.LogAction("CreateRowValueImport end", this.GetType());
//                            }

//                            import.Status = ImportStatus.InitialProcessingCompleted;
//                            importManager.UpdateObject(import);

//                            if (import.SourceMappingImportId != null)
//                            {
//                                byte[] campaignColumnIdHash = null;
//                                importManager.AutomapColumns(import.ObjectId, ref campaignColumnIdHash);

//                                using (CampaignManager campaignManager = new CampaignManager())
//                                {
//                                    campaignManager.CreateCampaigns(import.ObjectId, import.CountryId, campaignColumnIdHash, import.CreateUser);
//                                }
//                                import.CampaignColumnNameHash = campaignColumnIdHash;
//                                import.Status = ImportStatus.CampaignIdentifierSelected;
//                                importManager.UpdateObject(import);
//                            }
//                        }
//                    }
//                    catch (Exception exc)
//                    {
//                        import.Status = ImportStatus.Failed;
//                        import.Message = string.Format("File initial processing failed. {0}", this.GetMessage(exc));
//                        importManager.UpdateObject(import);
//                        importManager.DbContext.DeleteFailedImportData(import.ObjectId);
//                        throw new Exception(string.Format("File initial processing failed for import with ObjectId = '{0}'.", import.ObjectId), exc);
//                    }
//                }
//            }
//        }

//        private void DistinctProcessing()
//        {
//            using (ImportManager importManager = new ImportManager())
//            {
//                List<ImportBO> imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == ImportStatus.CampaignIdentifierSelected);

//                foreach (ImportBO import in imports)
//                {
//                    try
//                    {
//                        using (CmtFileSystemTempDirectory directory = new CmtFileSystemTempDirectory(ApplicationSettings.FileStorageFolder))
//                        {
//                            import.Status = ImportStatus.DistinctProcessingStarted;
//                            importManager.UpdateObject(import);


//                            var fileName = import.FileUniqueName + Path.GetExtension(import.FileName);
//                            var filePath = Path.Combine(ApplicationSettings.FileStorageFolder, fileName);
//                            if (!File.Exists(filePath))
//                            {
//                                var configuration = ApplicationSettings.ProcessedFileArchiveConfiguration;
//                                S3Helper.DownloadFile(configuration, S3Helper.GetPath(configuration.Directory, import.FileUniqueName + Path.GetExtension(import.FileName)), ApplicationSettings.FileStorageFolder, import.FileUniqueName.ToString());
//                            }



//                            Location sourceLocation = new FileSystemLocation(Path.Combine(ApplicationSettings.FileStorageFolder, Path.ChangeExtension(import.FileUniqueName.ToString().ToLower(), Path.GetExtension(import.FileName))));
//                            DataDistinctorService service = new DataDistinctorService(directory, GetFileStore, false);

//                            using (CampaignManager campaignManager = new CampaignManager())
//                            {
//                                using (DistinctRowManager distinctRowManager = new DistinctRowManager())
//                                {
//                                    using (DistinctRowGroupingManager distinctRowGroupingManager = new DistinctRowGroupingManager())
//                                    {
//                                        Guid importId = import.ObjectId;
//                                        List<DistinctRowBO> distinctRows = distinctRowManager.GetObjectsUsingBOPredicate(o => o.ImportId == importId && o.IsHeader);
//                                        int[] groupingColumns = { distinctRows.Single(o => o.ValueHash.SequenceEqual(import.CampaignColumnNameHash) ).ColumnIndex };
//                                        int[] columnsToAnalyze = distinctRows.Select(o => o.ColumnIndex).Except(groupingColumns).ToArray();
//                                        Dictionary<int, string> columnNames = distinctRows.ToDictionary(o => o.ColumnIndex, o => o.ColumnName);
//                                        Dictionary<int, GroupedDistinctValues> distinctValues = service.GetDistinctValues(sourceLocation, this.GetDataFormat(import), groupingColumns, columnsToAnalyze);
//                                        string campaignColumnName = distinctRows.Single(o => o.ValueHash.SequenceEqual(import.CampaignColumnNameHash)).ColumnName;

//                                        var campaigns = campaignManager.GetCampaignNamesWithIdsByImportId(importId);


//                                        distinctRowGroupingManager.DeleteByImportId(importId);

//                                        distinctRowGroupingManager.BeginBulkUpload();
//                                        DataTable dataTable = distinctRowGroupingManager.CreateEmptyDataTable();
//                                        bool recordsLeft = true;
//                                        int startIndex = 0;
//                                        int processedRecords = 0;

//                                        List<DistinctRowGroupingBO> rowsToInsert = new List<DistinctRowGroupingBO>();

//                                        foreach (int key in distinctValues.Keys)
//                                        {
//                                            foreach (string groupingValues in distinctValues[key].DistinctValues.Keys.Where(p => !string.IsNullOrWhiteSpace(p)))
//                                            {
//                                                Guid? campaignId = null;
//                                                if (campaigns.ContainsKey(groupingValues.Trim().ToLower()))
//                                                    campaignId = campaigns[groupingValues.Trim().ToLower()];

//                                                foreach (object value in distinctValues[key].DistinctValues[groupingValues])
//                                                {
//                                                    if (value != null)
//                                                    {
//                                                        rowsToInsert.Add(new DistinctRowGroupingBO()
//                                                        {
//                                                            DistinctRowId = this.GenerateObjectId(import.ObjectId, columnNames[key], value.ToString()),
//                                                            GroupingDistinctRowId = this.GenerateObjectId(import.ObjectId, campaignColumnName, groupingValues),
//                                                            ImportId = importId,
//                                                            CampaignId = campaignId
//                                                        });
//                                                    }
//                                                }
//                                            }
//                                        }

//                                        do
//                                        {
//                                            for (int i = startIndex; i < processedRecords + this.cacheSize; i++)
//                                            {
//                                                if (rowsToInsert.Count <= i)
//                                                {
//                                                    recordsLeft = false;
//                                                    break;
//                                                }

//                                                DataRow tableRow = dataTable.NewRow();
//                                                tableRow["DistinctRowId"] = rowsToInsert[i].DistinctRowId;
//                                                tableRow["GroupingDistinctRowId"] = rowsToInsert[i].GroupingDistinctRowId;
//                                                tableRow["ImportId"] = rowsToInsert[i].ImportId;
//                                                tableRow["CampaignId"] = (object) rowsToInsert[i].CampaignId ?? DBNull.Value;
//                                                dataTable.Rows.Add(tableRow);

//                                                processedRecords = i;
//                                            }

//                                            startIndex = processedRecords + 1;

//                                            distinctRowGroupingManager.UploadRecords(dataTable);
//                                            dataTable.Rows.Clear();
//                                        } while (recordsLeft);

//                                        distinctRowGroupingManager.EndBulkUpload();
//                                        distinctRowGroupingManager.DbContext.CommandTimeout = 1200;

//                                        logger.LogAction("CreateCampaignMapping start", this.GetType());
//                                        distinctRowGroupingManager.DbContext.CreateCampaignMapping(importId);

//                                        using (ColumnHeaderManager chm = new ColumnHeaderManager()) {
//                                            var headers = chm.GetObjectsUsingBOPredicate(o => o.ImportId == importId && o.ElementId.HasValue);

//                                            foreach (var columnHeader in headers) {
//                                                distinctRowGroupingManager.DbContext.UpdateCampaignMapping(importId, columnHeader.ElementId);
//                                            }

//                                        }

//                                        logger.LogAction("CreateCampaignMapping end", this.GetType());
//                                    }

//                                    import.Status = ImportStatus.DistinctProcessingCompleted;
//                                    importManager.UpdateObject(import);
//                                }
//                            }
//                        }
//                    }
//                    catch (Exception exc)
//                    {
//                        import.Status = ImportStatus.Failed;
//                        import.Message = string.Format("File distinct processing failed. {0}", this.GetMessage(exc));
//                        importManager.UpdateObject(import);
//                        importManager.DbContext.DeleteFailedImportData(import.ObjectId);
//                        throw new Exception(string.Format("File distinct processing failed for import with ObjectId = '{0}'.", import.ObjectId), exc);
//                    }
//                }
//            }
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
//            using (var sha1 = SHA1.Create())
//            {
//                string text = string.Format("{0}{1}{2}", importId.ToString().ToLower(), columnName, value);
//                byte[] hash = sha1.ComputeHash(Encoding.UTF32.GetBytes(text));
//                return new Guid(hash.Take(16).ToArray());
//            }
//        }

//        private string GetMessage(Exception exc)
//        {
//            if (exc.InnerException != null)
//                return this.GetMessage(exc.InnerException);

//            return exc.Message;
//        }
//    }
//}
