using System.Net;
using Web.Presentation.Exceptions;
using Web.Presentation.Router;

namespace Web.Presentation.Middleware;

public static class AuthorizationMiddleware
{
    public static void UseAuthorization(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            var route = context.Request.Route;
            var claims = context.Request.UserClaims;
            
            if (route != null)
            {
                if (claims == null && route.AuthRequirements.AuthMode == AuthMode.Required)
                {
                    ExceptionsHelpers.ThrowUnauthorized();
                }

                var requiredRole = route.AuthRequirements.RequiredRole;
                
                if (claims != null)
                {
                    if (requiredRole != null && claims.Role != requiredRole)
                    {
                        ExceptionsHelpers.ThrowForbidden();
                    }
                    
                    if (route.AuthRequirements.AuthMode == AuthMode.GuestsOnly)
                    {
                        ExceptionsHelpers.ThrowForbidden(error: "already_authenticated");
                        return;
                    }
                }
            }
            
            await (next?.Invoke(context) ?? Task.CompletedTask);
        });
    } 
}