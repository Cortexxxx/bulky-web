using System.Net;
using Application.Exceptions;
using Application.Services;
using Domain.Models;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Router;
using WebException = Web.Presentation.Exceptions.WebException;

namespace Web.Presentation.Middleware;

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
                    ExceptionsHelpers.ThrowUnauthorized();
                }

                Session session = default;
                
                try
                {
                    session = await authService.GetUserSessionAsync(tokenCookie.Value, context.CancellationToken);
                }
                catch (NoActiveSessionsException)
                {
                    ExceptionsHelpers.ThrowUnauthorized();
                }
                
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
                    ExceptionsHelpers.ThrowUnauthorized();
                }
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        });
    }
}