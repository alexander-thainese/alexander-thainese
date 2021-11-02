using CMT.BO;
using CMT.Common.Interfaces;
using System;

namespace CMT.PV.Security
{

    public partial class UserRoleBO : BaseObject, IBusinessObject
    {
        #region Field Names

        public partial class FieldNames
        {
            public const string UserId = "UserId";
            public const string RoleId = "RoleId";
            public const string Role = "Role";
            public const string User = "User";
            public const string UserRoleId = "UserRoleId";
        }

        #endregion

        public Guid ObjectId
        {
            get;
            set;
        }
        public Guid UserId
        {
            get;
            set;
        }

        public Guid RoleId
        {
            get;
            set;
        }

        public RoleBO Role
        {
            get;
            set;
        }

        public UserBO User
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