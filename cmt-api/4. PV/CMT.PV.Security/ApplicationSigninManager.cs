using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CMT.PV.Security
{
    public class ApplicationSignInManager : SignInManager<UserBO, Guid>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }


        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            if (UserManager == null)
            {
                return SignInStatus.Failure;
            }
            UserBO user = await UserManager.FindByNameAsync(userName).WithCurrentCulture();
            if (user == null)
            {
                return SignInStatus.Failure;
            }
            if (await UserManager.IsLockedOutAsync(user.Id).WithCurrentCulture())
            {
                return SignInStatus.LockedOut;
            }
            if (await UserManager.CheckPasswordAsync(user, password).WithCurrentCulture())
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id).WithCurrentCulture();
                return await SignInOrTwoFactor(user, isPersistent).WithCurrentCulture();
            }
            if (shouldLockout)
            {
                // If lockout is requested, increment access failed count which might lock out the user
                await UserManager.AccessFailedAsync(user.Id).WithCurrentCulture();
                if (await UserManager.IsLockedOutAsync(user.Id).WithCurrentCulture())
                {
                    return SignInStatus.LockedOut;
                }
            }
            return SignInStatus.Failure;
        }

        private async Task<SignInStatus> SignInOrTwoFactor(UserBO user, bool isPersistent)
        {
            string id = Convert.ToString(user.Id);
            if (await UserManager.GetTwoFactorEnabledAsync(user.Id).WithCurrentCulture()
                && (await UserManager.GetValidTwoFactorProvidersAsync(user.Id).WithCurrentCulture()).Count > 0
                && !await AuthenticationManager.TwoFactorBrowserRememberedAsync(id).WithCurrentCulture())
            {
                ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                AuthenticationManager.SignIn(identity);
                return SignInStatus.RequiresVerification;
            }
            await SignInAsync(user, isPersistent, false).WithCurrentCulture();
            return SignInStatus.Success;
        }

        public void SignIn(Guid userId)
        {
            string id = Convert.ToString(userId);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
            AuthenticationManager.SignIn(identity);
        }
    }

    public static class ApplicationUserExtensions
    {
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this UserBO user, ApplicationUserManager manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            ClaimsIdentity userIdentity = await manager.CreateIdentityAsync(user, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
