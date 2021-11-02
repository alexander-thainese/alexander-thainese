//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using CMT.BL;
//using CMT.BL.Core;
//using CMT.BO;
//using Newtonsoft.Json;

//namespace CMT
//{
//    public class StructureDiscoveryWorker : BaseWorker
//    {
//        public StructureDiscoveryWorker(ISystemLogger logger)
//            : base(logger)
//        {

//        }

//        protected override void ProcessWork()
//        {
//            List<ImportBO> imports;

//            using (ImportManager importManager = new ImportManager())
//            {
//                //imports = importManager.GetObjectsUsingBOPredicate(o => o.Status == Common.ImportStatus.FileReady);
//                Guid id = new Guid("51C53EB3-4B9B-E611-942A-00155D034947");
//                imports = importManager.GetObjectsUsingBOPredicate(o => o.ObjectId == id);
//            }

//            foreach (ImportBO import in imports)
//            {
//                WebClient webClient = new WebClient();
//                string text = webClient.DownloadString(string.Format("http://localhost:15198/api/Distinctor/DiscoverStructure/{0}/{1}", import.FileName, import.FileType));

//                using (DistinctRowManager distinctRowManager = new DistinctRowManager())
//                {
//                    Table table = JsonConvert.DeserializeObject<Table>(text);

//                    foreach (Column column in table.Columns)
//                    {
//                        DistinctRowBO distinctRow = new DistinctRowBO()
//                        {
//                            ColumnName = column.Name,
//                            Value = column.Name,
//                            IsHeader = true,
//                            ImportId = import.ObjectId,
//                            LineNumber = 0
//                        };

//                        distinctRowManager.InsertObject(distinctRow);
//                    }
//                }

//                using (ImportManager importManager = new ImportManager())
//                {
//                    import.Status = Common.ImportStatus.ColumnRead;
//                    importManager.UpdateObject(import);
//                }
//            }
//        }

//        protected override TimeSpan GetThreadLoopDelay()
//        {
//            return new TimeSpan(0, 0, 30);
//        }
//    }

//    public class Column
//    {
//        public string Name { get; set; }
//        public int Index { get; set; }
//        public Type Type { get; set; }
//        public bool Required { get; set; }
//    }

//    public class Table
//    {
//        public List<Column> Columns { get; set; }
//        public int Index { get; set; }
//    }
//}