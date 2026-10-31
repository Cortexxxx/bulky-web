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
                context.Response.WriteJson(new { email = (string[]) ["email_exists"] });
            }
            catch (NoActiveSessionsException)
            {
                context.Response.StatusCode = HttpStatusCode.Unauthorized;
                context.Response.WriteJson(new { error = (string[]) ["unauthorized"] });
            }
            catch (WrongEmailOrPasswordException)
            {
                context.Response.StatusCode = HttpStatusCode.Unauthorized;
                context.Response.WriteJson(new { error = (string[]) ["wrong_email_or_password"] });
            }
            catch (ValidationException e)
            {
                context.Response.StatusCode = HttpStatusCode.BadRequest;
                var obj = e.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(f => f.Key.ToLower(), 
                        failures => failures
                            .Select(f => f.ErrorCode)
                            .ToArray());
                context.Response.WriteJson(obj);
            }
        });
    }
}