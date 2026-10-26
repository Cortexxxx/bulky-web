namespace Bulky.Router;

public class RouteTemplate(string route)
{
    public string Route { get; private init; } = route;
    public readonly string[] RouteParts = WebHelpers.GetPathParts(route);
}