using CMT.Handlers;
using Microsoft.Owin.Security.OAuth;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;

namespace CMT
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            string origins = GetAllowedOrigins();
            EnableCorsAttribute cors = new EnableCorsAttribute(origins, "*", "*");
            config.EnableCors(cors);


            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new CmtAuthorizationFilterAttribute());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            config.Services.Replace(typeof(IExceptionHandler), new CMTExceptionHandler());

        }

        private static string GetAllowedOrigins()
        {
            return ConfigurationManager.AppSettings["CorsUrls"];
        }

    }
}
