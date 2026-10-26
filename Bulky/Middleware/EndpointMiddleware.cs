namespace Bulky.Middleware;

public static class EndpointMiddleware
{
    public static void UseEndpoints(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            var route = context.Request.Route;
            var endpointHandler = route?.EndpointHandler;

            if (endpointHandler != null)
            {
                await endpointHandler.Invoke(context);
            }
            else
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
        });
    }
}