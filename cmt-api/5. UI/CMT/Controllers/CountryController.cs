using CMT.BL;
using CMT.BO;
using System.Collections.Generic;
using System.Web.Http.Cors;

namespace CMT.Controllers
{

    [EnableCors("*", "Origin, Content-Type, Accept, Authorization, Cache-control, Pragma, Expires",
                                                "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class CountryController : BaseApiController
    {
        // GET api/<controller>
        /// <summary>
        /// Gets lsit of countries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CountryBO> Get()
        {
            using (CountryManager mgr = new CountryManager())
            {
                return mgr.GetObjects();
            }
        }

    }
}