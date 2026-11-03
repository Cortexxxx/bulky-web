using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Models.Auth;
using Web.Presentation.Router;

namespace Web.Presentation.Http;

public static class WebHelpers
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public static string[] GetPathParts(string path) => path
            .Replace('\\', '/')
            .Split('/')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

    public static JsonSerializerOptions? GetJsonSerializerOptions() => Options;
    
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