namespace Web.Presentation.Router;

public sealed record Route(
    HttpMethod Method,
    RouteTemplate Template,
    AuthRequirements AuthRequirements,
    EndpointHandler EndpointHandler,
    RouteKind RouteKind);