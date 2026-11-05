using Web.Http;

namespace Web.Routing;

public delegate Task EndpointHandler(HttpContext context);

public sealed record Route(
    HttpMethod Method,
    RouteTemplate Template,
    AuthRequirements AuthRequirements,
    EndpointHandler EndpointHandler,
    RouteKind RouteKind);