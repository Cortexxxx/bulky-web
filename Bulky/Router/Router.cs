namespace Bulky.Router;

public class Router(Route[] endpoints)
{
    public Route? Resolve(string localPath, HttpMethod method)
    {

        var parts = WebHelpers.GetPathParts(localPath);

        var partsCount = parts.Count();
        var possibleRoutes = endpoints
            .Where(r => r.Method == method && r.Template.RouteParts.Length == partsCount);
        
        // Тут маршрут разобран и отобраны те что совпадают по длине
        Route? foundEndpoint = null;
        foreach (var possibleRoute in possibleRoutes)
        {
            // Проверка точного совпадения

            var isCoincides = true;
            for (var i = 0; i < partsCount; i++)
            {
                if (parts[i] != possibleRoute.Template.RouteParts[i])
                {
                    // тут надо проверять на параметр, но не сразу, а когда все проверяться
                    isCoincides = false;
                }
            }

            if (isCoincides)
            {
                return possibleRoute;
            }
        }

        return null;
    }
}