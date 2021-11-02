using CMT.PV.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CMT.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            _publicClientId = publicClientId ?? throw new ArgumentNullException("publicClientId");
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            ApplicationUserManager userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            UserBO user;

            try
            {
                user = await userManager.FindAsync(context.UserName, context.Password);
            }
            catch (Exception e)
            {
                throw e;
            }
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager,
               OAuthDefaults.AuthenticationType);

            if (context.Scope.Any(p => !string.IsNullOrEmpty(p)))
            {
                oAuthIdentity.AddClaim(new Claim(ClaimsType.CountryCode, context.Scope.Single()));
            }

            if (user.ApplicationId.HasValue)
            {
                oAuthIdentity.AddClaim(new Claim(ClaimsType.ApplicationId, user.ApplicationId.ToString()));
            }

            ClaimsIdentity cookiesIdentity = await user.GenerateUserIdentityAsync(userManager,
                CookieAuthenticationDefaults.AuthenticationType);
            AuthenticationProperties properties = CreateProperties(user.UserName);

            IList<string> userRoles = await userManager.GetRolesAsync(user.Id);


            foreach (string role in userRoles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    oAuthIdentity.AddClaim(new Claim(ClaimsType.Role, role));
                }
            }

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            context.Validated(ticket);
            context.Request.Context.Authentication.SignIn(cookiesIdentity);
        }

        private string GetOAuthRoles(ClaimsIdentity oAuthIdentity)
        {
            return string.Join("|", oAuthIdentity.Claims.Where(p => p.Type == ClaimsType.Role).Select(p => p.Value).Distinct());
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            string roles = GetOAuthRoles(context.Identity);
            if (roles.Any())
            {
                context.AdditionalResponseParameters.Add("userRoles", roles);
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }

        public static AuthenticationProperties CreateProperties(string userName)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName },
            };
            //if (!string.IsNullOrEmpty(userRoles))
            //{
            //    data.Add("userRoles", userRoles);
            //}

            return new AuthenticationProperties(data);
        }
    }
}
