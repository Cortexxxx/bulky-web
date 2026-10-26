using System.Net;

namespace Bulky.Middleware;

public static class NotFoundMiddleware
{
    public static void UseNotFound(this Middleware middleware)
    {
        middleware.AddElement((_, _) => throw new WebException("Not found", HttpStatusCode.NotFound));
    }
}