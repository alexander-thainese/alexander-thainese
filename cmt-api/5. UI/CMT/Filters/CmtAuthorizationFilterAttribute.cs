using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Threading;
using System.Security.Claims;
using CMT.PV.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace CMT
{
    public class CmtAuthorizationFilterAttribute : IAuthenticationFilter
    {
        private const string SsoUsernameHeader = "IAMPFIZERUSERCN";

        public bool AllowMultiple => throw new NotImplementedException();

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return;
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var identity = FetchFromHeader(context);
            var owinContext = HttpContext.Current.Request.GetOwinContext();

            if (identity != null)
            {
                ApplicationUserManager userManager = owinContext.GetUserManager<ApplicationUserManager>();
                UserBO user;

                user = await userManager.FindByNameAsync(identity);

                if (user == null)
                {
                    return;
                }

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
                   OAuthDefaults.AuthenticationType);

                if (user.ApplicationId.HasValue)
                {
                    oAuthIdentity.AddClaim(new Claim(ClaimsType.ApplicationId, user.ApplicationId.ToString()));
                }

                ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                IList<string> userRoles = await userManager.GetRolesAsync(user.Id);


                foreach (string role in userRoles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        oAuthIdentity.AddClaim(new Claim(ClaimsType.Role, role));
                    }
                }

                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, null);
                owinContext.Authentication.SignIn(cookiesIdentity);

                context.Principal = new ClaimsPrincipal(cookiesIdentity);
            }
        }

        private string FetchFromHeader(HttpAuthenticationContext httpAuthenticationContext)
        {
            if(httpAuthenticationContext.Request.Headers.Contains(SsoUsernameHeader))
            {
                return httpAuthenticationContext.Request.Headers.Single(p => p.Key == SsoUsernameHeader).Value.Single();
            }

            return null;
        }
    }
}