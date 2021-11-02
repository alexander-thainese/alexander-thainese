using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CMT.BO
{
    [XmlType(TypeName = "Error")]
    public class DataRowError
    {
        public class FieldNames
        {
            public const string Code = "Code";
            public const string Severity = "Severity";
            public const string Message = "Message";
            //public const string FileColumnName = "FileColumnName";
            //public const string DbColumnName = "DbColumnName";
            public const string FileColumnNamesConcatenation = "FileColumnNamesConcatenation";
            public const string DbColumnNameConcatenation = "DbColumnNameConcatenation";
        }

        [XmlAttribute]
        public int ErrorGroupCode { get; set; }

        [XmlAttribute]
        public int Code { get; set; }

        [XmlIgnore]
        public FileLoadValidationStatus Severity { get; set; }

        [XmlAttribute(AttributeName = "Severity")]
        public int SeverityInt
        {
            get { return (int)Severity; }
            set { Severity = (FileLoadValidationStatus)value; }
        }

        [XmlAttribute]
        public string Message { get; set; }

        [XmlArray(ElementName = "DbColumnNames"), XmlArrayItem("Name")]
        public List<string> DbColumnNames { get; set; }

        [XmlIgnore]
        protected List<string> FileColumnNames { get; set; }

        [XmlIgnore]
        protected List<int> FileColumnIndices { get; set; }

        [XmlIgnore]
        public string FileColumnNamesConcatenation
        {
            get
            {
                if (FileColumnNames != null)
                {
                    return string.Join(", ", GetFileColumnNames());
                }

                return string.Join(", ", GetFileColumnIndices());
            }
        }

        [XmlIgnore]
        public string DbColumnNamesConcatenation { get { return string.Join(", ", GetDbColumnNames()); } }


        //[XmlIgnore]
        //public int? ColumnIndex { get; set; }

        public DataRowError() { }

        public DataRowError(int code, int groupCode, string fileColumnName = null)
            : this(code, groupCode, new[] { fileColumnName })
        { }

        public DataRowError(int code, int groupCode, string fileColumnName = null, string dbColumnName = null, string message = null)
            : this(code, groupCode, new[] { fileColumnName }, new[] { dbColumnName }, message)
        { }

        public DataRowError(int code, int errorGroupCode, IEnumerable<int> fileColumnIndices = null, IEnumerable<string> dbColumnNames = null, string message = null, FileLoadValidationStatus severity = FileLoadValidationStatus.Error)
            : this(code, errorGroupCode, (IEnumerable<string>)null, dbColumnNames, message, severity)
        {
            FileColumnIndices = fileColumnIndices == null ? null : fileColumnIndices.ToList();
        }

        public DataRowError(int code, int errorGroupCode, IEnumerable<string> fileColumnNames = null, IEnumerable<string> dbColumnNames = null, string message = null, FileLoadValidationStatus severity = FileLoadValidationStatus.Error)
        {
            Code = code;
            ErrorGroupCode = errorGroupCode;
            FileColumnNames = fileColumnNames == null ? null : fileColumnNames.ToList();
            DbColumnNames = dbColumnNames == null ? null : dbColumnNames.ToList();
            Message = message;
            Severity = severity;
        }

        public IReadOnlyCollection<string> GetDbColumnNames()
        {
            return DbColumnNames == null ? new List<string>().AsReadOnly() : DbColumnNames.AsReadOnly();
        }

        public void AddDbColumnName(string dbColumnName)
        {
            if (DbColumnNames == null)
            {
                DbColumnNames = new List<string>();
            }

            DbColumnNames.Add(dbColumnName);
        }

        public IReadOnlyCollection<string> GetFileColumnNames()
        {
            return FileColumnNames == null ? new List<string>().AsReadOnly() : FileColumnNames.AsReadOnly();
        }

        public IReadOnlyCollection<int> GetFileColumnIndices()
        {
            return FileColumnIndices == null ? new List<int>().AsReadOnly() : FileColumnIndices.AsReadOnly();
        }

        public void AddFileColumnName(string fileColumnName)
        {
            if (FileColumnNames == null)
            {
                FileColumnNames = new List<string>();
            }

            FileColumnNames.Add(fileColumnName);
        }

        public void AddFileColumnIndex(int fileColumnIndex)
        {
            if (FileColumnIndices == null)
            {
                FileColumnIndices = new List<int>();
            }

            FileColumnIndices.Add(fileColumnIndex);
        }
    }
}
