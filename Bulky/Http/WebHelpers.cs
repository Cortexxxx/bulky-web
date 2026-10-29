using System.Text;
using System.Text.Json;
using Bulky.Router;

namespace Bulky.Http;


public static class WebHelpers
{
    private static readonly JsonSerializerOptions? JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    public static void SetContentType(HttpResponse response, string extension)
    {
        response.Headers["Content-Type"] = extension switch
        {
            ".html" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            ".png" => "image/png",
            ".jpeg" => "image/jpeg",
            ".jpg" => "image/jpeg",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
    
    public static string[] GetPathParts(string path) => path
            .Replace('\\', '/')
            .Split('/')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

    public static JsonSerializerOptions? GetJsonSerializerOptions() => JsonSerializerOptions;
    
    public static Role GetRoleByString(string stringRole)
    {
        return stringRole switch
        {
            "user" => Role.User,
            "admin" => Role.Admin,
            _ => throw new ArgumentException("Unknown role or incorrect format", nameof(stringRole))
        };
    }

    public static RouteKind GetRouteKind(HttpRequest request)
    {
        if (request.Route == null)
        {
            return request.Url.LocalPath.StartsWith("/api/", StringComparison.Ordinal) 
                ? RouteKind.Api 
                : RouteKind.Page;
        }

        return request.Route.RouteKind;
    }
    
}