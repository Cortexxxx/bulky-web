namespace Bulky.Router.RouteResolutions;

public abstract class RouteResolution { }

public class UnknownRouteResolution : RouteResolution;

public class ForbiddenMethodRouteResolution(IEnumerable<HttpMethod> allowedMethods) : RouteResolution
{
    public IReadOnlyList<HttpMethod> AllowedMethods { get; } = allowedMethods.OrderBy(m => m.Method).ToList();
}

public class MatchedRouteResolution(Route route, IReadOnlyDictionary<string, string> parameters) : RouteResolution
{
    public Route Route { get; } = route;
    public readonly IReadOnlyDictionary<string, string> Parameters = parameters;
}