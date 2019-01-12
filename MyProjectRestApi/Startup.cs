using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Cors;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using MyProjectRestApi.Models;
using Owin;

[assembly: OwinStartup(typeof(MyProjectRestApi.Startup))]

namespace MyProjectRestApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.UseCors(CorsOptions.AllowAll);
            ConfigureAuth(app);
            ConfigureSignalR(app);

            //app.MapSignalR();
            //app.Map("/negotiate", map =>
            //{
            //    map.UseCors(CorsOptions.AllowAll);
            //});
            //app.MapHubs<NotifyHub>("/chat");
            //var options = new EnableCorsAttribute("http://localhost:4200", "*", "*");

            //app.UseCors(CorsOptions.AllowAll);
        }
    }
}
