using Web.Http;

namespace Web.Routing;

public class RouteTemplate(string route)
{
    public string Route { get; private init; } = route;
    public readonly string[] RouteParts = WebHelpers.GetPathParts(route);
}