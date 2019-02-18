using System.Web.Http;
using Microsoft.Owin.Security.OAuth;


namespace MyProjectRestApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Konfiguracja i usługi składnika Web API
            // Skonfiguruj składnik Web API, aby korzystał tylko z uwierzytelniania za pomocą tokenów bearer.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //var cors = new EnableCorsAttribute("http://localhost:62747", "*", "*");
            //config.EnableCors(cors);

            // Trasy składnika Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}/{numberPost}",
                defaults: new { id = RouteParameter.Optional, numberPost = RouteParameter.Optional }
            );

        }
    }
}
