//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;
//using Excel;

//namespace CMT.BL.DataMigrator
//{
//    public abstract class ExcelDataFileReader : DataFileReaderBase
//    {
//        private readonly Lazy<IExcelDataReader> excelDataReader;

//        protected ExcelDataFileReader(Stream dataStream, FileReadSettings settings)
//            : base(dataStream, settings)
//        {
//            this.excelDataReader = new Lazy<IExcelDataReader>(() => this.CreateDataReader());
//        }

//        public override void Dispose()
//        {
//            base.Dispose();
//            if (this.excelDataReader.IsValueCreated && this.excelDataReader.Value != null)
//                this.excelDataReader.Value.Dispose();
//        }

//        protected abstract IExcelDataReader CreateDataReader();

//        protected override void SetActiveFileSection(string fileSectionName)
//        {
//            this.excelDataReader.Value.SetCurrentWorksheet(fileSectionName);
//        }

//        protected override List<string> GetFileSectionNames()
//        {
//            return this.excelDataReader.Value.GetWorksheetNames();
//        }

//        protected override object GetCurrentRowValueForCell(int index)
//        {
//            return this.excelDataReader.Value.GetValue(this.ColumnIndices[index] - 1);
//        }

//        protected override bool AdvanceToNextRow()
//        {
//            //    base.AdvanceToNextRow();

//            //    int totalRowsToSkip = this.FirstDataRowNumber - 1;
//            //    if (this.Settings.UseColumnHeader)
//            //        totalRowsToSkip--;

//            //    var a =  this.excelDataReader.Value;
//            //    System.Diagnostics.Debug.WriteLine(this.CurrentRowNumber + "  " + a.Depth); 
//            //    if (a.Depth < this.CurrentRowNumber - totalRowsToSkip)
//            //    {
//            //        return true;
//            //    }
//            //    return this.excelDataReader.Value.Read();

//            base.AdvanceToNextRow();
//            return this.excelDataReader.Value.Read();

//        }

//        protected override List<string> ReadWholeCurrentRowValuesAsText()
//        {
//            List<string> currentRowTextValues = new List<string>();
//            int fieldCount = this.excelDataReader.Value.FieldCount;
//            for (int i = 0; i < fieldCount; i++)
//                currentRowTextValues.Add(this.excelDataReader.Value.GetString(i));

//            return currentRowTextValues;
//        }

//        protected override bool SupportsSections
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public override Dictionary<int, string> GetSections()
//        {
//            Dictionary<int, string> dict = new Dictionary<int, string>();
//            var sections = GetFileSectionNames();
//            for (int i = 0; i < sections.Count; i++)
//            {
//                dict.Add(i, sections[i]);
//            }
//            return dict;
//        }
//    }
//}
