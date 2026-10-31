using System.Net;
using Web.Presentation.Exceptions;
using WebException = Web.Presentation.Exceptions.WebException;

namespace Web.Presentation.Middleware;

public static class NotFoundMiddleware
{
    public static void UseNotFound(this Web.Presentation.Middleware.Middleware middleware)
    {
        middleware.AddElement((_, _) => throw new WebException("Not found", HttpStatusCode.NotFound));
    }
}