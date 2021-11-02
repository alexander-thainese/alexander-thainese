using CMT.BO;
using System;

namespace CMT.PV.Security
{
    public partial class UserClaimBO : BaseObject
    {
        #region Field Names

        public partial class FieldNames
        {
            public const string UserId = "UserId";
            public const string ClaimType = "ClaimType";
            public const string ClaimValue = "ClaimValue";
            public const string User = "User";
            public const string UserClaimId = "UserClaimId";
        }

        #endregion

        public Guid UserId
        {
            get;
            set;
        }

        public string ClaimType
        {
            get;
            set;
        }

        public string ClaimValue
        {
            get;
            set;
        }

        public UserBO User
        {
            get;
            set;
        }
    }
}