using CMT.DL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;

namespace CMT.PV.Security
{
    public class ApplicationUserManager : UserManager<UserBO, Guid>
    {
        public ApplicationUserManager(IUserStore<UserBO, Guid> store)
            : base(store)
        {
            UserTokenProvider = new TotpSecurityStampBasedTokenProvider<UserBO, Guid>();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            ApplicationUserManager manager = new ApplicationUserManager(new UserStore(context.Get<CmtEntities>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<UserBO, Guid>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            IDataProtectionProvider dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<UserBO, Guid>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = new TimeSpan(3, 0, 0)
                    };
            }
            return manager;
        }
    }
}
