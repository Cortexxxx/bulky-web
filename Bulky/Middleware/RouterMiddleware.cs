namespace Bulky.Middleware;

public static class RouterMiddleware
{
    public static void UseRouting(this Middleware middleware, Router.Router router)
    {
        middleware.AddElement((async (context, next) =>
        {
            // Route
            context.Request.Route = router.Resolve(context.Request.Url.LocalPath, context.Request.Method);
            await (next?.Invoke(context) ?? Task.CompletedTask);
        }));
    }
}