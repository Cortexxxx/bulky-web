using Web.Presentation.Exceptions;

namespace Web.Presentation.Middleware;

public static class NotFoundMiddleware
{
    public static void UseNotFound(this Middleware middleware)
    {
        middleware.AddElement((_, _) =>
        {
            ExceptionsHelpers.ThrowNotFound();
            return Task.CompletedTask;
        });
    }
}