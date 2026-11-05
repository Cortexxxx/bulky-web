using Web.Exceptions;

namespace Web.Pipeline;

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