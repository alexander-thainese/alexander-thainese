//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;
//using CMT.Common;
//using CMT.DL;

//namespace CMT.BL.DataMigrator
//{
//    /// <summary>
//    /// Setting for reading file
//    /// </summary>
//    public class FileReadSettings
//    {
//        /// <summary>
//        /// Constructor used to initialize new object
//        /// </summary>
//        public FileReadSettings()
//        {
//            this.FileEncoding = Encoding.Default;
//            this.ColumnSeparator = ColumnSeparators.None;
//            this.TextSeparator = TextSeparators.None;
//            this.UseColumnHeader = true;
//            //DateFormat = "yyyy-MM-dd";
//            this.FromRow = 1;
//            this.LastRowsToSkip = 0;
//            this.DbFormatNumeric = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
//        }

//        public FileReadSettings(ImportBO import)
//        {
//            this.FileEncoding = Encoding.Default;
//            this.ColumnSeparator = ColumnSeparators.Semicolon;
//            this.TextSeparator = TextSeparators.None;
//            this.UseColumnHeader = true;
//            //DateFormat = "yyyy-MM-dd";
//            this.FromRow = 1;
//            this.LastRowsToSkip = 0;
//            this.DbFormatNumeric = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
//        }

//        internal NumberFormatInfo DbFormatNumeric { get; private set; }

//        /// <summary>
//        /// Has first row contains headers, if yes column name will be get from that row, otherwise name has format Column{CellNo}
//        /// </summary>
//        public bool UseColumnHeader { get; set; }

//        /// <summary>
//        /// Number of section where to start reading (No of sheet in excel file)
//        /// </summary>
//        public string FileSection { get; set; }

//        /// <summary>
//        /// Number of row where to start reading
//        /// </summary>
//        public int FromRow { get; set; }

//        /// <summary>
//        /// Number of rows from the end to skip; 0 means no limit
//        /// </summary>
//        public int LastRowsToSkip { get; set; }

//        /// <summary>
//        /// Column separator
//        /// </summary>
//        public ColumnSeparators ColumnSeparator { get; set; }

//        /// <summary>
//        /// File encoding
//        /// </summary>
//        public Encoding FileEncoding { get; set; }

//        /// <summary>
//        /// Text separator
//        /// </summary>
//        public TextSeparators TextSeparator { get; set; }

//        /// <summary>
//        /// Decimal separator for numbers
//        /// </summary>
//        public string DecimalSeparator
//        {
//            get { return this.DbFormatNumeric.NumberDecimalSeparator; }
//            set { this.DbFormatNumeric.NumberDecimalSeparator = value; }
//        }

//        /*/// <summary>
//        /// Date format (available markers for digits: y - year, M - month, d - day). Default format: yyyy-MM-dd
//        /// </summary>
//        public string DateFormat { get; set; }*/

//        /// <summary>
//        /// Column separator in string version - used to split record into cells
//        /// </summary>
//        public string ColumnSeparatorTxt
//        {
//            get
//            {
//                switch (this.ColumnSeparator)
//                {
//                    default:
//                    case ColumnSeparators.Semicolon:
//                        return ";";
//                    case ColumnSeparators.Comma:
//                        return ",";
//                    case ColumnSeparators.Tab:
//                        return "\t";
//                    case ColumnSeparators.Space:
//                        return " ";
//                    case ColumnSeparators.Pipe:
//                        return "|";
//                    case ColumnSeparators.None:
//                        return string.Empty;
//                }
//            }
//        }

//        /// <summary>
//        /// Text separator in string version - used to split record into cells
//        /// </summary>
//        public string TextSeparatorTxt
//        {
//            get
//            {
//                switch (this.TextSeparator)
//                {
//                    default:
//                    case TextSeparators.None:
//                        return string.Empty;
//                    case TextSeparators.Quotation:
//                        return "\"";
//                }
//            }
//        }

//        /// <summary>
//        /// Get file encoding from number
//        /// </summary>
//        /// <param name="enc">Number of enconding</param>
//        /// <returns>File encoding</returns>
//        public static Encoding GetEncoding(int enc)
//        {
//            switch (enc)
//            {
//                case 1:
//                    return Encoding.ASCII;
//                case 2:
//                    return Encoding.BigEndianUnicode;
//                case 3:
//                    return Encoding.Unicode;
//                case 4:
//                    return Encoding.UTF32;
//                case 5:
//                    return Encoding.UTF7;
//                case 6:
//                    return Encoding.UTF8;
//                case 0:
//                default:
//                    return Encoding.Default;
//            }
//        }

//        /// <summary>
//        /// Get number of file encoding
//        /// </summary>
//        /// <param name="enc">File encoding</param>
//        /// <returns>Number of enconding</returns>
//        public static int GetEncoding(Encoding enc)
//        {
//            if (enc == Encoding.ASCII)
//                return 1;
//            else if (enc == Encoding.BigEndianUnicode)
//                return 2;
//            else if (enc == Encoding.Unicode)
//                return 3;
//            else if (enc == Encoding.UTF32)
//                return 4;
//            else if (enc == Encoding.UTF7)
//                return 5;
//            else if (enc == Encoding.UTF8)
//                return 6;
//            else
//                return 0;
//        }
//    }
//}
