using System.Net;
using System.Text;
using System.Text.Json;

namespace Bulky.Http;

public class HttpResponse
{
    public HttpResponse(HttpListenerResponse httpListenerResponse)
    {
        Headers = [];
    }

    public Dictionary<string, string> Headers { get; private init; }
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public byte[] Output { get; private set; } = [];
    public bool WasOutputWritten => Output.Length != 0;

    public void WriteBody(ReadOnlySpan<byte> buffer, string contentTypeExtension)
    {
        Output = buffer.ToArray();
        WebHelpers.SetContentType(this, contentTypeExtension);
    }

    public void WriteJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Output = Encoding.UTF8.GetBytes(json);
        WebHelpers.SetContentType(this, ".json");
    }
}