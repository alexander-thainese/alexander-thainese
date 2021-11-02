using CMT.BO;
using CMT.Common.Interfaces;
using System;

namespace CMT.PV.Security
{
    public partial class RoleBO : BaseObject, IBusinessObject
    {
        #region Field Names

        public partial class FieldNames
        {
            public const string Name = "Name";
            public const string ObjectId = "ObjectId";
        }

        #endregion

        #region Max field Lengths

        public partial class MaxFieldLengths
        {
            public const int Name = 256;
        }

        #endregion

        public Guid ObjectId { get; set; }

        public string Name
        {
            get;
            set;
        }

        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }
    }
}