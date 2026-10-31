using System.Net;
using System.Text.Json;
using System.Text;

namespace Web.Presentation.Http;

public class HttpResponse
{
    private readonly HttpListenerResponse _httpListenerResponse;

    public HttpResponse(HttpListenerResponse httpListenerResponse)
    {
        _httpListenerResponse = httpListenerResponse;
        Headers = [];
    }

    public Dictionary<string, string> Headers { get; private init; }
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public byte[] Output { get; private set; } = [];

    public void WriteBody(ReadOnlySpan<byte> buffer, string contentTypeExtension)
    {
        Output = buffer.ToArray();
        SetContentType(contentTypeExtension);
        SetContentLength(Output.Length);
    }

    public void WriteJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, WebHelpers.GetJsonSerializerOptions());

        Output = Encoding.UTF8.GetBytes(json);
        SetContentType(".json");
        SetContentLength(Output.Length);
    }

    public void SetCookie(string name, string value, CookieOptions cookieOptions)
    {
        _httpListenerResponse.AppendHeader("Set-Cookie", $"{name}={value};{cookieOptions}");
    }

    public void SetContentLength(int length)
    {
        _httpListenerResponse.ContentLength64 = length;
    }
    
    public void SetContentType(string extension)
    {
        _httpListenerResponse.ContentType = extension switch
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
}