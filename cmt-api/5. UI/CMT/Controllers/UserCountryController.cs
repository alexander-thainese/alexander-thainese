using CMT.Attributes;
using CMT.BO;
using CMT.PV.Security;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    [EnableCors("*", "Origin, Content-Type, Accept, Authorization, Cache-control, Pragma, Expires",
                                                "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    [CMTAuthorize]
    [Route("api/user-countries")]
    public class UserCountryController : BaseApiController
    {
        private readonly UserCountryManager UserCountryManager;

        public UserCountryController(UserCountryManager userCountryManager)
        {
            UserCountryManager = userCountryManager;
        }

        [HttpGet]
        [Route("api/user-countries")]
        public async Task<IHttpActionResult> GetObjects()
        {
            List<CountryBO> countries = await UserCountryManager.GetUserCountries(AuthenticatedUserId);
            return Ok(countries);
        }
    }
}