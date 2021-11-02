//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CMT.BO;

//namespace CMT.BL.DataMigrator
//{
//    public interface IFileReader : IDisposable
//    {
//        Dictionary<int, string> GetSections();
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="readFile">If method should read file or depend on previous read values</param>
//        /// <returns></returns>
//        List<string> GetColumnNames(bool readFile = false);

//        /// <summary>
//        /// Read file and return all rows as IEnumerable collection.
//        /// </summary>
//        /// <param name="useFileFormats">If true return rows as they are in file, otherwise use formats from settings</param>
//        /// <returns>Collection of rows</returns>
//        IEnumerable<List<string>> ReadFile(bool useFileFormats);

//        /// <summary>
//        /// Read file and return specified amount of rows as IEnumerable collection.
//        /// </summary>
//        /// <param name="useFileFormats">If true return rows as they are in file, otherwise use formats from settings</param>
//        /// <param name="rowCount">Number of rows to return (if 0 - returns all rows)</param>
//        /// <returns>Collection of rows</returns>
//        IEnumerable<List<string>> ReadFile(bool useFileFormats, int rowCount);


//        IEnumerable<RawValueDataRow> ReadNextRows(int count);
//    }
//}
