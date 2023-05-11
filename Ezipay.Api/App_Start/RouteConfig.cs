using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ezipay.Api
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            //old
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //routes.MapPageRoute("Tickets", "Response/Tickets", "~/WebForms/Tickets/Report.aspx");

            routes.MapHttpRoute(
           name: "Error404",
           routeTemplate: "{*url}",
           defaults: new { controller = "Error", action = "Handle404" }
               );

        }
    }
}
