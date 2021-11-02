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
//    public class ExcelBinaryDataFileReader : ExcelDataFileReader
//    {
//        protected override bool SupportsSections { get { return true; } }

//        public ExcelBinaryDataFileReader(Stream dataStream, FileReadSettings settings)
//            : base(dataStream, settings)
//        { }

//        protected override IExcelDataReader CreateDataReader()
//        {
//            return ExcelReaderFactory.CreateBinaryReader(this.DataStream);
//        }
//    }
//}
