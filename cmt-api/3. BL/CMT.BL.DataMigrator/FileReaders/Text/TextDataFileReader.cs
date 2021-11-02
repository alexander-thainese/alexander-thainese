//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;
//using CMT.Common;
//using CMT.DL;

//namespace CMT.BL.DataMigrator
//{
//    public class TextDataFileReader : DataFileReaderBase
//    {
//        private string[] currentRowValues;
//        private Lazy<StreamReader> streamReader;

//        protected override bool SupportsSections { get { return false; } }

//        protected override void InitializeReader()
//        {
//            this.streamReader = new Lazy<StreamReader>(() => GetStream());
//        }


//        private StreamReader GetStream()
//        {

//            //https://www.nuget.org/packages/UDE.CSharp
//            Ude.CharsetDetector cdet = new Ude.CharsetDetector();
//            cdet.Feed(DataStream);
//            cdet.DataEnd();
//            Encoding enc = Encoding.UTF8;
//            if (!string.IsNullOrEmpty(cdet.Charset))
//                enc = Encoding.GetEncoding(cdet.Charset);
//            //revert stream to 0 
//            this.DataStream.Position = 0;
//            return new StreamReader(this.DataStream, enc);
//        }
//        public TextDataFileReader(Stream dataStream, FileReadSettings settings, bool encode = true)
//            : base(dataStream, settings, encode)
//        { }



//        public override void Dispose()
//        {
//            base.Dispose();
//            if (this.streamReader.IsValueCreated && this.streamReader.Value != null)
//                this.streamReader.Value.Dispose();
//        }

//        protected override bool AdvanceToNextRow()
//        {
//            base.AdvanceToNextRow();
//            string line = this.streamReader.Value.ReadLine();
//            if (line == null)
//                return false;

//            if (Settings.TextSeparator == TextSeparators.None)
//            {

//                this.currentRowValues = line.Split(new[] { this.Settings.ColumnSeparatorTxt }, StringSplitOptions.None);
//            }
//            else
//            {
//                if (this.Settings.ColumnSeparatorTxt.Length > 1 || Settings.TextSeparatorTxt.Length > 1)
//                    throw new ArgumentException("Text separator and Column separater must be single character");

//                char columnSeparator = this.Settings.ColumnSeparator == ColumnSeparators.None ? '\0' : Settings.ColumnSeparatorTxt[0];
//                this.currentRowValues = SplitQuoted(line, columnSeparator, Settings.TextSeparatorTxt[0]);
//                //  this.currentRowValues = SplitQuotedOld(line, this.Settings.ColumnSeparatorTxt, Settings.TextSeparatorTxt);


//            }
//            return true;
//        }

//        private static string[] SplitQuotedOld(string input, string separator, string quotechar)
//        {
//            List<string> tokens = new List<string>();

//            StringBuilder sb = new StringBuilder();
//            bool escaped = false;
//            foreach (char c in input)
//            {
//                if (c.ToString().Equals(separator) && !escaped)
//                {
//                    // we have a token
//                    tokens.Add(sb.ToString().Trim());
//                    sb.Clear();
//                }
//                else if (c.ToString().Equals(separator) && escaped)
//                {
//                    // ignore but add to string
//                    sb.Append(c);
//                }
//                else if (c.ToString().Equals(quotechar))
//                {
//                    escaped = !escaped;
//                }
//                else
//                {
//                    sb.Append(c);
//                }
//            }
//            tokens.Add(sb.ToString().Trim());

//            return tokens.ToArray();
//        }

//        public static string[] SplitQuoted(string input, char separator, char quotechar)
//        {

//            List<string> columns = new List<string>();
//            List<int> separatorIndexes = new List<int>();
//            List<int> quoteSeparatorIndexes = new List<int>();
//            List<int> breakOnIndexes = new List<int>();
//            int startIndex = 0;
//            int count = 0;
//            int endIndex = 0;

//            //znajdz wszystkie separaotry kolumn i text markery
//            for (int i = 0; i < input.Length; i++)
//            {
//                if (input[i].Equals(separator))
//                {
//                    separatorIndexes.Add(i);
//                }
//                else if (input[i].Equals(quotechar))
//                {
//                    quoteSeparatorIndexes.Add(i);
//                }
//            }

//            //znajdz separatory kolumn ktore nie sa pomiedzy text markerami
//            if (separatorIndexes.Count != 0)
//            {

//                if (quoteSeparatorIndexes.Count == 0)
//                {
//                    breakOnIndexes.AddRange(separatorIndexes);
//                }
//                else
//                {
//                    foreach (var currentSeparatorIndex in separatorIndexes)
//                    {
//                        bool isInsideQuotes = false;
//                        if (currentSeparatorIndex > quoteSeparatorIndexes.Last())
//                        {
//                            breakOnIndexes.Add(currentSeparatorIndex);
//                        }
//                        else
//                        {

//                            //sprawdzic czy separator nie jest pomiedzy cudzyslowiem
//                            for (int i = 0; i < quoteSeparatorIndexes.Count(); i += 2)
//                            {

//                                if (i + 1 >= quoteSeparatorIndexes.Count())
//                                {
//                                    isInsideQuotes = isInsideQuotes || false;
//                                }
//                                else if (quoteSeparatorIndexes[i] < currentSeparatorIndex && (quoteSeparatorIndexes[i + 1] > currentSeparatorIndex))
//                                {

//                                    isInsideQuotes = true;
//                                }

//                            }
//                            if (!isInsideQuotes)
//                            {
//                                breakOnIndexes.Add(currentSeparatorIndex);
//                            }
//                        }

//                    }
//                }
//            }

//            //gdy nie ma separatorow
//            if (breakOnIndexes.Count == 0)
//            {
//                startIndex = 0;
//                count = input.Length;
//                endIndex = startIndex + count - 1;
//                SaveColumnValue(input, quotechar, columns, ref startIndex, ref count, endIndex);

//            }
//            else
//            {

//                for (int i = 0; i < breakOnIndexes.Count(); i++)
//                {
//                    //pierwsza kolumna
//                    if (i == 0)
//                    {
//                        startIndex = 0;
//                        count = breakOnIndexes[i];
//                        endIndex = startIndex + count - 1;
//                        if (count == 0)
//                        {
//                            columns.Add(null);
//                        }
//                        else
//                        {
//                            SaveColumnValue(input, quotechar, columns, ref startIndex, ref count, endIndex);
//                        }
//                    }
//                    //kolejne kolumny
//                    else if (i - 1 >= 0)
//                    {
//                        startIndex = breakOnIndexes[i - 1] + 1;//kolejny znak po separatorze
//                        count = breakOnIndexes[i] - (breakOnIndexes[i - 1] + 1);//ilosc znakow pomiedzy separatorami
//                        endIndex = startIndex + count - 1;
//                        SaveColumnValue(input, quotechar, columns, ref startIndex, ref count, endIndex);
//                    }
//                    //jezeli ostatni separator koulumn to reszta stringa jako kolumna
//                    if (i == breakOnIndexes.Count - 1)
//                    {
//                        startIndex = breakOnIndexes[i] + 1;
//                        count = input.Length - startIndex;
//                        endIndex = startIndex + count - 1;
//                        if (count == 0)
//                        {
//                            columns.Add(null);
//                        }
//                        else
//                        {
//                            SaveColumnValue(input, quotechar, columns, ref startIndex, ref count, endIndex);
//                        }
//                    }

//                }
//            }
//            return columns.ToArray();

//        }



//        private static void SaveColumnValue(string input, char quotechar, List<string> tokens, ref int startIndex, ref int count, int endIndex)
//        {
//            //sprawdzenie gdy linie ktore przychodza sa puste lub zaiwraj tylko separatory lub textmarkery
//            var length = input.Length;
//            if (length == 0 || length <= startIndex || length < endIndex || endIndex < 0)
//                return;
//            try
//            {
//                //jezeli poczatek i koniec to text markery to nie bierzemy ich pod uwagę
//                if (input[startIndex] == quotechar && input[endIndex] == quotechar)
//                {
//                    startIndex++;
//                    count = count - 2;
//                }
//                //usunac podwoje text markery
//                tokens.Add(input.Substring(startIndex, count).Replace(quotechar.ToString() + quotechar.ToString(), quotechar.ToString()));
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }
//        }



//        protected override List<string> ReadWholeCurrentRowValuesAsText()
//        {
//            return this.currentRowValues.ToList();
//        }

//        protected override object GetCurrentRowValueForCell(int index)
//        {

//            if (this.ColumnIndices[index] - 1 >= this.currentRowValues.Length)
//                return null;

//            return this.currentRowValues[this.ColumnIndices[index] - 1];
//        }
//        public override Dictionary<int, string> GetSections()
//        {
//            var dct = new Dictionary<int, string>();
//            dct.Add(0, "File");
//            return dct;
//        }
//        //public static Encoding DetermineEncoding(byte[] fileContent)
//        //{
//        //    UniversalDetector charsetDetector = new UniversalDetector();
//        //    charsetDetector.HandleData(fileContent);
//        //    Encoding encoding = Encoding.GetEncoding(charsetDetector.DetectedCharsetName);
//        //    return encoding;
//        //}
//    }
//}
