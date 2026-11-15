using System.Net;
using Application;
using Application.Services;
using Domain.Models.Auth;

namespace Web.Pipeline;

public static class AuthenticationMiddleware
{
    public static void UseAuthentication(this Middleware middleware, AuthService authService)
    {
        middleware.AddElement(async (context, next) =>
        {
            var request = context.Request;
            var route = context.Request.Route;
            
            var tokenCookie = request.GetCookie("token");
            
            if (route != null)
            {
                var claims = await GetClaimsAsync(authService, tokenCookie, context.CancellationToken);
                request.UserClaims = claims;
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
                
        var user = await authService.GetUserByIdAsync(session.UserId, ct);
        
        return new UserClaims
        {
            Role = user.Role,
            Username = user.Username,
            Email = user.Email,
            Id = user.Id,
            CreatedAt = user.CreatedAt,
        };
    }
}
