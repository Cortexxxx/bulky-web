using System.Net;
using System.Text.Json;
using Bulky.Exceptions;
using Bulky.Models;
using Bulky.Router;
using WebException = Bulky.Exceptions.WebException;

namespace Bulky.Http;

public class HttpRequest
{
    private readonly HttpListenerRequest _request;

    public HttpRequest(HttpListenerRequest request)
    {
        if (request.Url == null)
        {
            ExceptionsHelpers.ThrowBadRequest("Url is null", "null_url");
        }
        
        _request = request;

        Url = request.Url;
        Method = HttpMethod.Parse(request.HttpMethod);
    }

    public Uri Url { get; private set; }
    public HttpMethod Method { get; set; }
    public IReadOnlyDictionary<string, string>? RouteParameters { get; set; }
    public Route? Route { get; set; }
    public UserClaims? UserClaims;
    private byte[]? _bodyBytes;
    
    public async Task<T> ReadJsonAsync<T>(CancellationToken ct)
    {
        var body = await ReadBodyAsync(ct);
        
        if (body.Length == 0) ExceptionsHelpers.ThrowBadRequest("Body is empty", "body_required");
        
        try
        {
            var obj = JsonSerializer.Deserialize<T>(body,
                WebHelpers.GetJsonSerializerOptions());
            
            if (obj == null) ExceptionsHelpers.ThrowBadRequest("Body is null", "not_null_required");

            return obj;
        }
        catch (JsonException e)
        {
            throw new WebException("Json deserialization error", HttpStatusCode.BadRequest, "invalid_json", e);
        }
    }

    private async Task<byte[]> ReadBodyAsync(CancellationToken ct)
    {
        if (_bodyBytes != null) return _bodyBytes;
        using var bodyStream = new MemoryStream();

        if (_request.ContentLength64 > WebConstants.MaxPayloadLength)
            ExceptionsHelpers.ThrowWebException("Payload is too long", HttpStatusCode.RequestEntityTooLarge, "long_payload");

        long totalRead = 0;
        var buffer = new byte[4096];
        
        while (true)
        {
            var read = await _request.InputStream.ReadAsync(buffer, ct);
            
            if (read == 0) break;
            
            totalRead += read;
            
            if (totalRead > WebConstants.MaxPayloadLength) 
                ExceptionsHelpers.ThrowWebException("Payload is too long", HttpStatusCode.RequestEntityTooLarge, "long_payload");
            
            await bodyStream.WriteAsync(buffer.AsMemory()[..read], ct);
        }

        _bodyBytes = bodyStream.ToArray();
        return _bodyBytes;
    }

    public Cookie? GetCookie(string name)
    {
        return _request.Cookies[name];
    }
}