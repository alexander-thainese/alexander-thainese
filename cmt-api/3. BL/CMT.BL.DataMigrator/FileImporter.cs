//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BL.Core;
//using CMT.BO;
//using CMT.Common;
//using CMT.DL;

//namespace CMT.BL.DataMigrator
//{
//    public class FileImporter : IFileImporter
//    {
//        private FileReadSettings settings;
//        private int cacheSize = ApplicationSettings.LoadCacheSize;
//        private ISystemLogger logger;

//        public FileImporter(ISystemLogger logger = null)
//            : this(new FileReadSettings(), logger)
//        {
//        }

//        public FileImporter(FileReadSettings settings, ISystemLogger logger = null)
//        {
//            if (settings == null)
//                throw new ArgumentNullException(nameof(settings));

//            this.settings = settings;
//            this.logger = logger;
//        }

//        public FileImporter(FileReadSettings settings, int cacheSize, ISystemLogger logger = null)
//            : this(settings, logger)
//        {
//            if (cacheSize < 0)
//                throw new ArgumentOutOfRangeException(nameof(cacheSize));

//            this.cacheSize = cacheSize;
//        }

//        private IEnumerable<string> GetDuplicates(List<string> columns)
//        {
//            var duplicatedCols = columns
//                .GroupBy(p => p)
//                .Where(g => g.Count() > 1)
//                .Select(g => g.Key);

//            return duplicatedCols;
//        }

//        private DataRow GetSrcObjectFromRec(Guid importId, RawValueDataRow item, int lineNumber, DataTable dataTable)
//        {
//            DataRow tableRow = dataTable.NewRow();

//            tableRow["ObjectId"] = Guid.NewGuid();
//            tableRow["ImportId"] = importId;
//            tableRow["LineNumber"] = lineNumber;

//            if (item != null)
//            {
//                for (int i = 1; i <= item.Values.Length; i++)
//                {
//                    string value = i <= item.Values.Length ? item.Values[i - 1] == null ? null : item.Values[i - 1].ToString() : null;
//                    string columnName = string.Format(i < 10 ? "String0{0}" : "String{0}", i);
//                    tableRow[columnName] = value;
//                }
//            }

//            return tableRow;
//        }

//        private void RemoveEmptyRows(DataTable dataTable)
//        {
//            if (dataTable == null)
//                throw new ArgumentNullException(nameof(dataTable));

//            if (dataTable.Rows.Count == 0)
//                throw new ArgumentException(nameof(dataTable));

//            for (int i = 0; i < dataTable.Rows.Count; i++)
//            {
//                bool isEmpty = true;
//                DataRow row = dataTable.Rows[i];

//                for (int j = 0; j < dataTable.Columns.Count; j++)
//                {
//                    DataColumn column = dataTable.Columns[j];

//                    if (column.ColumnName == "ObjectId" || column.ColumnName == "ImportId" || column.ColumnName == "LineNumber")
//                        continue;

//                    isEmpty = IsEmptyCell(row[j]);

//                    if (!isEmpty)
//                        break;
//                }

//                if (isEmpty)
//                {
//                    dataTable.Rows.RemoveAt(i);
//                    i--;
//                }
//            }
//        }

//        private bool IsEmptyCell(object value)
//        {
//            string s = value?.ToString().Trim();
//            return string.IsNullOrEmpty(s);
//        }

//        private void LogError(Exception exc)
//        {
//            if (this.logger != null)
//                this.logger.LogError(this.GetType(), exc);
//        }
//    }
//}
