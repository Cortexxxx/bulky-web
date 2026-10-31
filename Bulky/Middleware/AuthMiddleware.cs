using System.Net;
using Bulky.Exceptions;
using Bulky.Models;
using Bulky.Router;
using Bulky.Services;
using WebException = Bulky.Exceptions.WebException;

namespace Bulky.Middleware;

public static class AuthMiddleware
{
    public static void UseAuthentication(this Middleware middleware, AuthService authService)
    {
        middleware.AddElement(async (context, next) =>
        {
            var request = context.Request;
            var route = context.Request.Route;
            if (route is { AuthRequirements.NeedAuth: true })
            {
                var tokenCookie = request.GetCookie("token");
        
                if (tokenCookie == null)
                {
                    throw new NoActiveSessionsException();
                }
        
                var session = await authService.GetUserSessionAsync(tokenCookie.Value, context.CancellationToken);
                var getUserResult = await authService.GetUserByIdAsync(session.UserId, context.CancellationToken);
                if (getUserResult != null)
                {
                    var user = getUserResult.Value;
                    request.UserClaims = new UserClaims
                    {
                        Role = user.Role,
                        Email = user.Email,
                        Id = user.Id,
                        CreatedAt = user.CreatedAt,
                    };
                }
                else
                {
                    switch (route.RouteKind)
                    {
                        case RouteKind.Page:
                            context.Response.StatusCode = HttpStatusCode.Redirect;
                            context.Response.Headers["Location"] = "/login";
                            context.Response.SetContentLength(0);
                            return;
                        case RouteKind.Api:
                            ExceptionsHelpers.ThrowUnauthorized();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        });
    }
}