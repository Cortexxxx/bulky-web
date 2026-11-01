using System.Net;
using Application.Exceptions;
using Application.Services;
using Domain.Models;
using Web.Presentation.Exceptions;
using Web.Presentation.Router;

namespace Web.Presentation.Middleware;

public static class AuthMiddleware
{
    public static void UseAuthentication(this Middleware middleware, AuthService authService)
    {
        middleware.AddElement(async (context, next) =>
        {
            var request = context.Request;
            var response = context.Response;
            var route = context.Request.Route;
            
            var tokenCookie = request.GetCookie("token");
            
            if (route != null)
            {
                var claims = await GetClaimsAsync(authService, tokenCookie, context.CancellationToken);
                request.UserClaims = claims;
                
                if (claims == null && route.AuthRequirements.AuthMode == AuthMode.Required)
                {
                    ExceptionsHelpers.ThrowUnauthorized();
                }
                
                if (claims != null && route.AuthRequirements.AuthMode == AuthMode.GuestsOnly)
                {
                    response.StatusCode = HttpStatusCode.Redirect;
                    response.Headers["Location"] = PathConstants.DashboardWorkoutPagePath;
                    response.SetContentLength(0);
                    return;
                }
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        });
    }
    
    private static async Task<UserClaims?> GetClaimsAsync(AuthService authService, Cookie? tokenCookie, CancellationToken ct)
    {
        if (tokenCookie == null) return null;

        Session session;
        
        try
        {
            session = await authService.GetUserSessionAsync(tokenCookie.Value, ct);
        }
        catch (NoActiveSessionsException)
        {
            return null;
        }
                
        var getUserResult = await authService.GetUserByIdAsync(session.UserId, ct);

        if (getUserResult == null) return null;
        
        var user = getUserResult.Value;
        
        return new UserClaims
        {
            Role = user.Role,
            Email = user.Email,
            Id = user.Id,
            CreatedAt = user.CreatedAt,
        };
    }
}