using CMT.Common.Interfaces;
using System;

namespace CMT.BO
{
    public class BrandImportHistoryBO : IBusinessObject
    {
        public Guid ObjectId { get; set; }

        public string FileName { get; set; }

        public DateTime Date { get; set; }

        public string DisplayName
        {
            get
            {
                return FileName;
            }
        }
    }
}
