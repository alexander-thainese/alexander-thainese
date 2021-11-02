//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;

//namespace CMT.BL.DataMigrator
//{
//    public abstract class DataFileReaderBase : IDisposable, IFileReader
//    {
//        private readonly bool throwOnMissingColumns;

//        private readonly Queue<object[]> possibleLastRowsToSkipQueue = new Queue<object[]>();

//        private DataRowError[] columnErrors;

//        public string[] ColumnHeaders { get; private set; }
//        private int firstDataRowNumber = -1;
//        private int currentFileSectionIndex = 0;
//        protected int FirstDataRowNumber
//        {
//            get
//            {
//                if (this.firstDataRowNumber > 0)
//                    return this.firstDataRowNumber;

//                this.firstDataRowNumber = this.Settings.FromRow;

//                if (this.ContainsValidationReportSection)
//                {
//                    this.firstDataRowNumber++;
//                    if (!this.Settings.UseColumnHeader)
//                        this.firstDataRowNumber++;
//                }

//                if (this.Settings.UseColumnHeader)
//                    this.firstDataRowNumber++;

//                return this.firstDataRowNumber;
//            }
//        }

//        protected int CurrentRowNumber { get; private set; }

//        protected int[] ColumnIndices { get; private set; }

//        protected Stream DataStream { get; private set; }

//        protected FileReadSettings Settings { get; private set; }

//        protected DataFileReaderBase(Stream datastream, FileReadSettings settings, bool encode = false)
//        {
//            this.Settings = settings;
//            this.throwOnMissingColumns = false;
//            this.InitializeReader();
//            this.DataStream = datastream;
//        }

//        public IEnumerable<RawValueDataRow> ReadToEnd()
//        {
//            if (this.SupportsSections)
//            {
//                this.EnsureFileSectionConsistency();
//                this.SetActiveFileSection();
//            }
//            foreach (RawValueDataRow rawTextDataRow in this.ReadNextRowsInternal(null))
//                yield return rawTextDataRow;

//        }

//        public IEnumerable<RawValueDataRow> ReadNextRows(int count)
//        {

//            if (this.SupportsSections)
//            {


//                this.EnsureFileSectionConsistency();
//                if (this.currentFileSectionIndex < 0 && !this.AdvanceToNextFileSection())
//                    yield break;
//            }

//            int rowCount = 0;

//            do
//            {
//                foreach (RawValueDataRow rawTextDataRow in this.ReadNextRowsInternal(count))
//                {
//                    yield return rawTextDataRow;
//                    rowCount++;
//                    if (rowCount == count)
//                        yield break;
//                }
//            }
//            while (this.SupportsSections && rowCount < count && this.AdvanceToNextFileSection());

//        }

//        public virtual void Dispose()
//        {
//            if (this.DataStream != null)
//                this.DataStream.Dispose();
//        }

//        #region FileSection

//        public const string ValidationReportFileSectionName = "BLU_VALIDATION_REPORT";


//        private string currentFileSectionName;

//        protected abstract bool SupportsSections { get; }

//        protected bool ContainsValidationReportSection
//        {
//            get
//            {
//                if (!this.SupportsSections)
//                    return false;

//                return this.GetFileSectionNames().Contains(ValidationReportFileSectionName);
//            }
//        }


//        protected virtual void SetActiveFileSection(string fileSectionName)
//        {
//            if (this.SupportsSections)
//                throw new NotImplementedException();

//            throw new NotSupportedException();
//        }
//        private void SetActiveFileSection()
//        {
//            var sections = this.GetFileSectionNames();
//            string currentSection = this.currentFileSectionName;
//            if (this.Settings.FileSection == null || !sections.Contains(this.Settings.FileSection))
//            {
//                this.currentFileSectionName = sections.First();
//                this.Settings.FileSection = this.currentFileSectionName;
//            }
//            else
//            {

//                this.currentFileSectionName = this.Settings.FileSection;
//            }
//            if (currentSection != this.currentFileSectionName)
//            {
//                this.SetActiveFileSection(this.currentFileSectionName);
//            }
//            this.possibleLastRowsToSkipQueue.Clear();
//        }

//        protected virtual List<string> GetFileSectionNames()
//        {
//            if (this.SupportsSections)
//                throw new NotImplementedException();

//            throw new NotSupportedException();
//        }

//        protected virtual void EnsureFileSectionConsistency()
//        {
//            if (this.currentFileSectionName != Settings.FileSection && !string.IsNullOrEmpty(Settings.FileSection))
//            {
//                SetActiveFileSection();
//            }

//        }


//        private bool AdvanceToNextFileSection()
//        {
//            return false;
//        }

//        #endregion

//        protected abstract List<string> ReadWholeCurrentRowValuesAsText();

//        protected void FillArrayWithCurrentRowValues(object[] rowValues)
//        {
//            for (int i = 0; i < rowValues.Length; i++)
//            {
//                if (this.Settings.UseColumnHeader || this.columnErrors[i] == null)
//                {
//                    var val = this.GetCurrentRowValueForCell(i);
//                    if (val != null)
//                    {
//                        rowValues[i] = val.ToString().Trim();
//                    }
//                    else
//                    {
//                        rowValues[i] = val;

//                    }
//                }

//            }
//        }

//        protected abstract object GetCurrentRowValueForCell(int index);

//        protected virtual bool AdvanceToNextRow()
//        {
//            this.CurrentRowNumber++;
//            return true;
//        }

//        protected virtual void InitializeReader() { }

//        protected void InitializeColumnIndicesFromHeader(List<string> columnHeaders)
//        {
//            List<int> columnIndices = new List<int>();
//            List<string> headers = new List<string>();

//            for (int index = 0; index < columnHeaders.Count; index++)
//            {
//                string columnHeader = columnHeaders[index];
//                headers.Add(string.IsNullOrWhiteSpace(columnHeader) ? index.ToString() : columnHeader.Trim());
//                columnIndices.Add(index + 1);
//            }

//            this.ColumnIndices = columnIndices.ToArray();
//            this.columnErrors = new DataRowError[this.ColumnIndices.Length];
//            this.ColumnHeaders = headers.ToArray();
//        }

//        private RawValueDataRow ProcessRow(IEnumerable<object> rowValues)
//        {
//            this.possibleLastRowsToSkipQueue.Enqueue(rowValues.ToArray());

//            List<DataRowError> dataRowErrors = new List<DataRowError>();
//            if (this.possibleLastRowsToSkipQueue.Count <= this.Settings.LastRowsToSkip)
//                return null;

//            object[] lastCachedRowValues = this.possibleLastRowsToSkipQueue.Dequeue();
//            object[] columnValues = new object[this.ColumnIndices.Length];
//            bool validRowLength = true;
//            for (int i = 0; i < this.ColumnIndices.Length; i++)
//            {
//                //todo ColumnIndex w error?
//                DataRowError columnError = this.columnErrors[i];
//                if (columnError != null)
//                {
//                    dataRowErrors.Add(columnError);
//                    continue;
//                }

//                if (i < 0 || i >= lastCachedRowValues.Length)
//                    validRowLength = false;
//                else columnValues[i] = lastCachedRowValues[i];
//            }

//            if (!validRowLength)
//                dataRowErrors.Add(new DataRowError(DataRowErrorCode.InvalidRowLength, DataRowErrorCode.ApplicationErrorsGroupCode, null));

//            int lineNumber = this.CurrentRowNumber - (this.FirstDataRowNumber - 1 + this.Settings.LastRowsToSkip);

//            RawValueDataRow rawValueDataRow = new RawValueDataRow(lineNumber, columnValues, this.currentFileSectionName);
//            rawValueDataRow.Errors.AddRange(dataRowErrors);
//            return rawValueDataRow;
//        }

//        private IEnumerable<RawValueDataRow> ReadNextRowsInternal(int? count)
//        {
//            if (count <= 0)
//                throw new ArgumentOutOfRangeException("count");

//            object[] rowValues = null;
//            while (this.AdvanceToNextRow())
//            {
//                int totalRowsToSkip = this.FirstDataRowNumber - 1;
//                if (this.Settings.UseColumnHeader)
//                    totalRowsToSkip--;

//                if (this.CurrentRowNumber <= totalRowsToSkip)
//                    continue;

//                if (this.ColumnIndices == null)
//                {
//                    List<string> columnHeaders = this.ReadWholeCurrentRowValuesAsText();

//                    try
//                    {
//                        this.InitializeColumnIndicesFromHeader(columnHeaders);
//                    }
//                    catch (Exception ex)
//                    {
//                        if (!this.ContainsValidationReportSection)
//                            throw;

//                        const string MessageToAppend = "This file contains validation report metadata. Please ensure that rows above first imported data row were not modified manually.";
//                        throw new Exception(string.Format("{0} {1}", ex.Message, MessageToAppend));
//                    }
//                }


//                if (rowValues == null)
//                    rowValues = new object[this.ColumnIndices.Length];

//                this.FillArrayWithCurrentRowValues(rowValues);

//                RawValueDataRow dataRow = this.ProcessRow(rowValues);
//                if (dataRow != null)
//                {
//                    yield return dataRow;
//                    if (count.HasValue && --count == 0)
//                        yield break;
//                }
//            }
//        }

//        public abstract Dictionary<int, string> GetSections();
//        public List<string> GetColumnNames(bool readFile = false)
//        {
//            if (readFile)
//            {
//                ReadNextRows(1).ToList();
//            }

//            if (ColumnIndices == null)
//            {
//                throw new Exception("Headers are not avaliable before file is read");
//            }

//            var headers = new List<string>();
//            if (this.Settings.UseColumnHeader)
//            {
//                foreach (var i in this.ColumnHeaders)
//                {
//                    headers.Add(i);
//                }
//            }
//            else
//            {

//                for (int i = 0; i < this.ColumnIndices.Count(); i++)
//                {
//                    headers.Add(string.Format("Column{0}", i + 1));
//                }

//            }
//            return headers;
//        }

//        public IEnumerable<List<string>> ReadFile(bool useFileFormats)
//        {
//            return new List<List<string>>();
//        }

//        public IEnumerable<List<string>> ReadFile(bool useFileFormats, int rowCount)
//        {
//            var b = ReadNextRows(rowCount).ToList();
//            var l = new List<List<string>>();
//            foreach (var i in b)
//            {
//                var item = new List<string>();
//                item.AddRange(i.Values.Select(a => a != null ? a.ToString() : string.Empty));
//                l.Add(item);
//            }
//            return l;
//        }
//    }
//}
