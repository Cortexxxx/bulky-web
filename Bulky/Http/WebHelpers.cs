using System.Net;
using System.Text.Json;
using Bulky.Http;

namespace Bulky;

public static class WebHelpers
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
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

    public static JsonSerializerOptions GetJsonSerializerOptions() => JsonSerializerOptions;

    public static void ThrowNotFound()
    {
        throw new WebException("Not found", HttpStatusCode.NotFound, "not_found");
    }
}