using System.Diagnostics.CodeAnalysis;
using Web.Presentation.Http;

namespace Web.Presentation.Router;

public class Router
{
    private readonly List<Route> _endpoints = [];
    private static readonly AuthRequirements DefaultAuthRequirements = new();
    private bool _isInitialized;

    public void Initialize()
    {
        ThrowIfInitialized();

        _isInitialized = true;
    }

    public void Map(HttpMethod method, string template, EndpointHandler endpointHandler, RouteKind routeKind) 
        => Map(method, template, endpointHandler, routeKind, DefaultAuthRequirements);
    
    public void Map(HttpMethod method, string template, EndpointHandler endpointHandler, RouteKind routeKind, AuthRequirements authRequirements)
    {
        ThrowIfInitialized();
        
        if (_endpoints.Any(e => e.Method == method && e.Template.Route == template))
        {
            throw new InvalidOperationException("Endpoint with same method and path is already exists");
        }
        
        _endpoints.Add(new Route(method, new RouteTemplate(template), authRequirements, endpointHandler, routeKind));
    }
    
    public RouteResolution Resolve(string localPath, HttpMethod method)
    {
        var parts = WebHelpers.GetPathParts(localPath);

        var partsCount = parts.Length;
        
        var possibleEntries = _endpoints
            .Where(r => r.Template.RouteParts.Length == partsCount).ToList();
        
        var allowedMethods = new HashSet<HttpMethod>();
        var matchedRoutes = new List<MatchedRouteResolution>();

        foreach (var possibleEntry in possibleEntries)
        {
            var isCoincides = true;
            var parameters = new Dictionary<string, string>();
            for (var i = 0; i < partsCount; i++)
            {
                var templateRoutePart = possibleEntry.Template.RouteParts[i];
                
                if (templateRoutePart.StartsWith('{') 
                    && templateRoutePart.EndsWith('}'))
                {
                    parameters[templateRoutePart[1..^1]] = parts[i];
                }
                else
                {
                    if (parts[i] == templateRoutePart) continue;
                    
                    isCoincides = false;
                }
            }

            if (!isCoincides) continue;

            
            allowedMethods.Add(possibleEntry.Method);

            if (possibleEntry.Method == method)
            {
                matchedRoutes.Add(new MatchedRouteResolution(possibleEntry, parameters));
            }
        }

        var matchedRouteResolution = matchedRoutes.MinBy(r => r.Parameters.Count);
        
        if (matchedRouteResolution is not null)
        {
            return matchedRouteResolution;
        }

        if (allowedMethods.Count != 0)
        {
            return new ForbiddenMethodRouteResolution(allowedMethods);
        }
        
        return new UnknownRouteResolution();
    }
    
    public static bool TryGetIntParameter(IReadOnlyDictionary<string, string>? parameters, 
        string parameterName, [NotNullWhen(true)] out int? param)
    {
        int value;
        param = null;
        
        if (parameters != null && parameters.TryGetValue(parameterName, out var stringParam))
        {
            var wasParsed = int.TryParse(stringParam, out value);
            
            if (!wasParsed) return false;
        }
        else
        {
            return false;
        }

        param = value;
        return true;
    }
    
    public static bool TryGetStringParameter(IReadOnlyDictionary<string, string>? parameters, 
        string parameterName, [NotNullWhen(true)] out string? param)
    {
        param = null;
        return parameters != null && parameters.TryGetValue(parameterName, out param);
    }
    
    public static bool TryGetGuidParameter(IReadOnlyDictionary<string, string>? parameters, 
        string parameterName, [NotNullWhen(true)] out Guid? param)
    {
        param = null;
        if (parameters != null && parameters.TryGetValue(parameterName, out var stringParam)
                               && Guid.TryParse(stringParam, out var guid))
        {
            param = guid;
            return true;
        }

        return false;
    }
    private void ThrowIfInitialized()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("Router is already initialized");
        }
    }
}