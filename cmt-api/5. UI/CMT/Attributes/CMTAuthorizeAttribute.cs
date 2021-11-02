using CMT.PV.Security;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace CMT.Attributes
{
    public class CMTAuthorizeAttribute : AuthorizeAttribute
    {
        public CMTAuthorizeAttribute()
        {
            AllowExternal = false;
        }

        /// <summary>
        /// Allow external users to access this resource, by default false
        /// </summary>
        public bool AllowExternal { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (base.IsAuthorized(actionContext))
            {
                ClaimsIdentity identity = ((ClaimsIdentity)actionContext.RequestContext.Principal.Identity);
                //verify allow null country Code is allowed for account
                ApplicationUserManager mgr = actionContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

                if (!AllowExternal && IsApplicationIdentity(identity))
                {
                    return false;
                }

                return true;
            }
            return HttpContext.Current.IsDebuggingEnabled;
        }

        /// <summary>
        /// Checks if identity contains ApplicationId Claim
        /// </summary>
        /// <param name="identity">Claims Identity of requestor</param>
        /// <returns></returns>
        private bool IsApplicationIdentity(ClaimsIdentity identity)
        {
            return identity.Claims.Any(p => p.Type == ClaimsType.ApplicationId
                && !string.IsNullOrEmpty(p.Value));
        }
    }
}