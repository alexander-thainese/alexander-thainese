using CMT.BO;
using CMT.Common.Interfaces;
using System;

namespace CMT.PV.Security
{
    public partial class UserCountryBO : BaseObject, IBusinessObject
    {
        public UserCountryBO()
        {
        }

        #region Field Names

        public partial class FieldNames
        {
            public const string UserId = "UserId";
            public const string CountryId = "CountryId";
        }

        #endregion

        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
        public Guid CountryId { get; set; }

        public string DisplayName => ToString();
    }
}