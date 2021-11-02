using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CMT.Controllers
{
    public class Product
    {
        public string Name { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    [EnableCors("*", "Origin, Content-Type, Accept, Authorization",
                                               "GET, PUT, POST, DELETE, OPTIONS", SupportsCredentials = true)]
    public class TestController : ApiController
    {
        // GET api/values
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            return "oldValue";
        }



        // POST api/values
        /// <summary>
        /// Saves product
        /// </summary>
        /// <param name="product">Product objhect</param>
        /// <returns></returns>
        [HttpPost]
        public string Post(Product product)
        {
            return product.Name;
        }
        /// <summary>
        /// TEst comment
        /// </summary>
        /// <returns>aaaa</returns>
        public HttpResponseMessage Options()
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}