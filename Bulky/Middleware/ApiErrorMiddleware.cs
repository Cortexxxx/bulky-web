using System.Net;
using Bulky.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace Bulky.Middleware;

public static class ApiErrorMiddleware
{
    public static void UseApiErrors(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            try
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
            catch (EmailAlreadyExistsException)
            {
                context.Response.StatusCode = HttpStatusCode.Conflict;
                context.Response.WriteJson(new { errors = new { email = (string[]) ["email_exists"] } });
            }
            catch (ValidationException e)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                var obj = e.Errors.GroupBy(f => f.PropertyName)
                    .ToDictionary(f => f.Key, failures => failures.ToArray());
                context.Response.WriteJson(obj);
            }
        });
    }
}