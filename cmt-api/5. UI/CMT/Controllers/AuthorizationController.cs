using CMT.Handlers;
using CMT.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    [EnableCors("*", "Origin, Content-Type, Accept, Authorization, Cache-control, Pragma, Expires",
                                              "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]

    public class AuthorizationController : BaseApiController
    {

        /// <summary>
        /// Creates token
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Route("api/authorization")]
        [HttpPost]
        public TokenContainer Post(TokenRequest request)
        {
            if (request != null && !string.IsNullOrEmpty(request.Scope))
            {
                if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Scope) || string.IsNullOrEmpty(request.Role))
                {
                    ApiResponse<string> retValue = new ApiResponse<string>();
                    retValue.IsSucceed = false;
                    retValue.Errors = new List<string>() { "Login information is not valid" };
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Unauthorized, retValue);
                    throw new HttpResponseException(response);
                }
            }

            else if (request == null || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.UserName))
            {

                ApiResponse<string> retValue = new ApiResponse<string>();
                retValue.IsSucceed = false;
                retValue.Errors = new List<string>() { "Login information is not valid" };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Unauthorized, retValue);
                throw new HttpResponseException(response);
            }


            HttpClient client = new HttpClient();
            List<KeyValuePair<string, string>> contentData = new List<KeyValuePair<string, string>>();
            contentData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            contentData.Add(new KeyValuePair<string, string>("username", request.UserName));
            contentData.Add(new KeyValuePair<string, string>("password", request.Password));
            contentData.Add(new KeyValuePair<string, string>("scope", request.Scope));
            contentData.Add(new KeyValuePair<string, string>("role", request.Role));

            FormUrlEncodedContent newContent = new FormUrlEncodedContent(contentData);
            string url = "http://" + Request.RequestUri.Host + ":" + Request.RequestUri.Port + "/Token";
            if (!string.IsNullOrEmpty(request.Role))
            {
                url += "?role=" + request.Role;
            }

            HttpResponseMessage result = client.PostAsync(url, newContent).Result;
            if (result.IsSuccessStatusCode)
            {
                string res = result.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(request.Scope))
                {
                    new CMTLogger().LogAction(string.Format("Token requested from {0} IP", Request.GetOwinContext().Request.RemoteIpAddress), GetType());
                }

                return JsonConvert.DeserializeObject<TokenContainer>(res);
            }
            else
            {
                ApiResponse<string> retValue = new ApiResponse<string>();
                retValue.IsSucceed = false;
                retValue.Errors = new List<string>() { "Access denied" };
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Unauthorized, retValue);
                throw new HttpResponseException(response);
            }
        }




    }
}
