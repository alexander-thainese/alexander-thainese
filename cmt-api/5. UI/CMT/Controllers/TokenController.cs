using CMT.Attributes;
using CMT.Models;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    [EnableCors("*", "Origin, Content-Type, Accept, Authorization",
                                               "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    [CMTAuthorize]
    public class TokenController : BaseApiController
    {

        public string Post(TokenContainer container)
        {
            return container.AccessToken;
        }
    }
}