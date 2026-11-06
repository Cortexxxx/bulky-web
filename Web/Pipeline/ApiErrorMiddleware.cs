using System.Net;
using Application;
using Web.Http;
using Web.Routing;
using ValidationException = FluentValidation.ValidationException;

namespace Web.Pipeline;

public static class ApiErrorMiddleware
{
    public static void UseApiErrors(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            if (context.Request.Route?.RouteKind == RouteKind.Api)
            {
                try
                {
                    await (next?.Invoke(context) ?? Task.CompletedTask);
                }
                catch (DomainException e)
                {
                    context.SetError(e);
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
            }
            else
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
        });
    }

    private static void SetError(this HttpContext context, DomainException e)
    {
        context.Response.StatusCode = e.GetStatusCode();
        context.Response.WriteJson(new { error = (string[]) [e.Error] });
    }
}