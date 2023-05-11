using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Net;
using System.Web.Http.Dispatcher;
using System.Web.Http.Controllers;
using Ezipay.Api.Filters;

namespace Ezipay.Api
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
           
            //old
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);           
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new HttpNotFoundAwareDefaultHttpControllerSelector(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpActionSelector), new HttpNotFoundAwareControllerActionSelector());
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ////Response.Redirect("https://app.ezeepaygh.com/api/comingsoon/underMaintenance");

            //string url = string.Empty;

            //if (Request.Url.Host.ToString().ToLower() != "localhost" && !Request.IsSecureConnection)
            //{
            //    url = Request.Url.ToString().ToLower().Replace("http:", "https:");
            //    //if (Request.Url.Host.ToString().ToLower().Contains("//ezeepaygh.com"))
            //    if (Request.Url.Host.ToString().ToLower().Contains("//ezipaygh.com"))
            //    {
            //        //Response.Redirect("https://www.ezeepaygh.com");
            //        Response.Redirect("https://www.ezipaygh.com");
            //    }
            //    else
            //    {
            //        Response.Redirect(url);
            //    }
            //}
            Response.Headers.Remove("Server");
            
        }
    }
}