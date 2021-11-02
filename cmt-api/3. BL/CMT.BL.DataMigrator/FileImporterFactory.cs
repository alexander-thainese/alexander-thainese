//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;

//namespace CMT.BL.DataMigrator
//{
//    public static class FileImporterFactory
//    {
//        public static FileReaderHandler GetFileReaderMethod;
//        public static ManagerHandler<IImportManager> GetImportManagerMethod;
//        public static ManagerHandler<ISourceTableManager> GetSourceTableManagerMethod;
//        public static ManagerHandler<ICampaignManager> GetCampaignManagerMethod;
//        public static ManagerHandler<ICampaignImportManager> GetCampaignImportManagerMethod;
//        private static readonly byte[] ZIP_XLSX = { 80, 75, 3, 4 };

//        public static IFileReader GetFileReader(ImportBO import, FileReadSettings settings)
//        {
//            if (GetFileReaderMethod != null)
//                return GetFileReaderMethod(import, settings);

//            ImportManager importManager = new ImportManager();
//            byte[] headerBytes;
//            var s = importManager.GetStream(import, out headerBytes);

//            switch (import.FileType)
//            {
//                default:
//                case FileType.Txt:
//                    return new TextDataFileReader(s, settings);
//                case FileType.Csv:
//                    return new CsvDataFileReader(s, settings);
//                case FileType.Xls:
//                    if (headerBytes.SequenceEqual(ZIP_XLSX))
//                        return new ExcelOpenXmlDataFileReader(s, settings);
//                    else
//                        return new ExcelBinaryDataFileReader(s, settings);
//                case FileType.Xlsx:
//                    if (headerBytes.SequenceEqual(ZIP_XLSX))
//                        return new ExcelOpenXmlDataFileReader(s, settings);
//                    else
//                        return new ExcelBinaryDataFileReader(s, settings);
//            }
//        }


//        private static Encoding DetermineEncoding(Stream str)
//        {
//            byte[] buf = new byte[4];
//            byte[] bufUTF8 = new byte[] { 0xEF, 0xBB, 0xBF };
//            byte[] bufUTF16BE = new byte[] { 0xFE, 0xFF };
//            byte[] bufUTF16 = new byte[] { 0xFF, 0xFE };
//            Dictionary<Encoding, byte[]> dict = new Dictionary<Encoding, byte[]>()
//            {
//                { Encoding.UTF8, bufUTF8 },
//                { Encoding.BigEndianUnicode, bufUTF16BE },
//                { Encoding.Unicode, bufUTF16}
//            };

//            str.Read(buf, 0, buf.Length);
//            str.Seek(0, SeekOrigin.Begin);

//            foreach (KeyValuePair<Encoding, byte[]> kvp in dict)
//            {
//                bool equal = true;
//                for (int i = 0; i < kvp.Value.Length; i++)
//                {
//                    if (buf[i] != kvp.Value[i])
//                    {
//                        equal = false;
//                        break;
//                    }
//                }
//                if (equal)
//                    return kvp.Key;
//            }

//            return Encoding.UTF8;
//        }

//        public static IImportManager GetImportManager()
//        {
//            if (GetImportManagerMethod != null)
//                return GetImportManagerMethod();

//            return new ImportManager();
//        }

//        public static ISourceTableManager GetSourceTableManager()
//        {
//            if (GetSourceTableManagerMethod != null)
//                return GetSourceTableManagerMethod();

//            return new SourceTableManager();
//        }

//        //public static ICampaignManager GetCampaignManager()
//        //{
//        //    if (GetCampaignManagerMethod != null)
//        //        return GetCampaignManagerMethod();

//        //    return new CampaignManager();
//        //}

//        public static ICampaignImportManager GetCampaignImportManager()
//        {
//            if (GetCampaignImportManagerMethod != null)
//                return GetCampaignImportManagerMethod();

//            return new CampaignImportManager();
//        }
//    }
//}
