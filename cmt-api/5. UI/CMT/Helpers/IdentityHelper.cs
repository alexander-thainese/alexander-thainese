using CMT.PV.Security;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace CMT.Helpers
{
    public static class IdentityHelper
    {
        public static string GetUserCountryCode(IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity.Claims.Single(p => p.Type == ClaimsType.CountryCode).Value;
        }
    }
}
