using PAS.API.ErrorHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using Unity;

namespace PAS.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
            // Web API routes
            //config.MapHttpAttributeRoutes(new EnableInheritDirectRouteProvider());
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "CoreApi",
                routeTemplate: "api/core/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            config.Services.Replace(typeof(IExceptionHandler), new PASExceptionHandler());

            //config.Filters.Add(new AuthorizeAttribute());
        }
    }
}
