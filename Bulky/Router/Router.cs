using System.Diagnostics.CodeAnalysis;

namespace Bulky.Router;

public class Router(IEnumerable<Route> endpoints)
{
    public (Route route, Dictionary<string, string>? parameters)? Resolve(string localPath, HttpMethod method)
    {

        var parts = WebHelpers.GetPathParts(localPath);

        var partsCount = parts.Length;
        
        var possibleRoutes = endpoints
            .Where(r => r.Method == method && r.Template.RouteParts.Length == partsCount);
        
        var possibleRoutesWithParams = new List<Route>();
        
        foreach (var possibleRoute in possibleRoutes)
        {

            var isCoincides = true;
            var canBeParamRoute = false;
            
            for (var i = 0; i < partsCount; i++)
            {
                var templateRoutePart = possibleRoute.Template.RouteParts[i];
                
                if (parts[i] == templateRoutePart) continue;
                
                if (templateRoutePart.StartsWith('{') 
                    && templateRoutePart.EndsWith('}'))
                {
                    canBeParamRoute = true;
                }
                else
                {
                    isCoincides = false;
                }
            }

            if (!isCoincides) continue;
            
            if (canBeParamRoute)
            {
                possibleRoutesWithParams.Add(possibleRoute);
            }
            else
            {
                return (possibleRoute, null);
            }
        }
        
        var possibleRouteWithParam = possibleRoutesWithParams.FirstOrDefault();

        if (possibleRouteWithParam == null) return null;
        
        var parameters = new Dictionary<string, string>();
            
        for (var i = 0; i < partsCount; i++)
        {
            var templateRoutePart = possibleRouteWithParam.Template.RouteParts[i];
            if (templateRoutePart.StartsWith('{')
                && templateRoutePart.EndsWith('}'))
            {
                parameters[templateRoutePart.Substring(1, templateRoutePart.Length - 2)] = parts[i];
            }
        }

        return (possibleRouteWithParam, parameters);
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
}