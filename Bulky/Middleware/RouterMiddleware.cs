using System.Net;
using Bulky.Router;
using WebException = Bulky.Exceptions.WebException;

namespace Bulky.Middleware;

public static class RouterMiddleware
{
    public static void UseRouting(this Middleware middleware, Router.Router router)
    {
        middleware.AddElement(async (context, next) =>
        {
            var routerResult = router.Resolve(context.Request.Url.LocalPath, context.Request.Method);
            
            switch (routerResult)
            {
                case MatchedRouteResolution routeResolution:
                    context.Request.Route = routeResolution.Route;
                    context.Request.RouteParameters = routeResolution.Parameters;
                    break;
                case ForbiddenMethodRouteResolution routeResolution:
                    context.Response.Headers["Allow"] = string.Join(", ", routeResolution.AllowedMethods.Select(m => m.Method));
                    throw new WebException("Method not allowed", HttpStatusCode.MethodNotAllowed, "method_not_allowed");
                case UnknownRouteResolution:
                    throw new WebException("Not found", HttpStatusCode.NotFound, "not_found");
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        });
    }
}