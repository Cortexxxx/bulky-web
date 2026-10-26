namespace Bulky.Router;

public sealed record Route(
    HttpMethod Method, 
    RouteTemplate Template, 
    AuthRequirements AuthRequirements, 
    EndpointHandler EndpointHandler);