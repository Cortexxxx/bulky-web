namespace Bulky.Middleware;

public static class RouterMiddleware
{
    public static void UseRouting(this Middleware middleware, Router.Router router)
    {
        middleware.AddElement((async (context, next) =>
        {
            var routerResult = router.Resolve(context.Request.Url.LocalPath, context.Request.Method);
            
            if (routerResult != null)
            {
                context.Request.Route = routerResult.Value.route;
                context.Request.RouteParameters = routerResult.Value.parameters;
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        }));
    }
}