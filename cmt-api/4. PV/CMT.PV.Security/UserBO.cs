using CMT.BO;
using CMT.Common.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;

namespace CMT.PV.Security
{
    public partial class UserBO : BaseObject, IUser<Guid>, IBusinessObject
    {
        public UserBO()
        {
            Claims = new List<UserClaimBO>();
            Logins = new List<UserLoginBO>();
            Roles = new List<UserRoleBO>();
        }
        /// <summary>
        /// For aspnet identity purpouse
        /// </summary>
        public Guid Id { get { return ObjectId; } }

        public List<UserClaimBO> Claims { get; set; }
        public List<UserLoginBO> Logins { get; set; }
        public List<UserRoleBO> Roles { get; set; }

        #region Field Names

        public partial class FieldNames
        {
            public const string Email = "Email";
            public const string EmailConfirmed = "EmailConfirmed";
            public const string PasswordHash = "PasswordHash";
            public const string SecurityStamp = "SecurityStamp";
            public const string PhoneNumber = "PhoneNumber";
            public const string PhoneNumberConfirmed = "PhoneNumberConfirmed";
            public const string TwoFactorEnabled = "TwoFactorEnabled";
            public const string LockoutEndDateUtc = "LockoutEndDateUtc";
            public const string LockoutEndDateUtcLocal = "LockoutEndDateUtcLocal";
            public const string LockoutEnabled = "LockoutEnabled";
            public const string AccessFailedCount = "AccessFailedCount";
            public const string UserName = "UserName";
            public const string SecurityQuestion = "SecurityQuestion";
            public const string SecurityQuestionAnswer = "SecurityQuestionAnswer";
            public const string UserProfiles = "UserProfiles";
            public const string UserId = "UserId";
        }

        #endregion

        #region Max field Lengths

        public partial class MaxFieldLengths
        {
            public const int Email = 256;
            public const int UserName = 256;
            public const int SecurityQuestion = 100;
            public const int SecurityQuestionAnswer = 50;
        }

        #endregion

        public Guid ObjectId { get; set; }
        public string Email { get; set; }

        public bool EmailConfirmed
        { get; set; }
        public string PasswordHash
        { get; set; }
        public string SecurityStamp
        { get; set; }
        public string PhoneNumber
        { get; set; }
        public bool PhoneNumberConfirmed
        { get; set; }
        public bool TwoFactorEnabled
        { get; set; }
        public DateTime? LockoutEndDateUtc
        { get; set; }
        public DateTime? LockoutEndDateUtcLocal
        { get; set; }
        public bool LockoutEnabled
        { get; set; }
        public int AccessFailedCount
        { get; set; }
        public string UserName
        { get; set; }
        public string SecurityQuestion
        { get; set; }
        public string SecurityQuestionAnswer
        { get; set; }

        public Guid? ApplicationId { get; set; }
        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

    }
}