using Domain.Models.Auth;

namespace Web.Presentation.Router;

public record AuthRequirements(AuthMode AuthMode = AuthMode.Public, Role? RequiredRole = null);
