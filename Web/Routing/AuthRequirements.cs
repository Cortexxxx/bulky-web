using Domain.Models.Auth;

namespace Web.Routing;

public record AuthRequirements(AuthMode AuthMode = AuthMode.Public, Role? RequiredRole = null);
