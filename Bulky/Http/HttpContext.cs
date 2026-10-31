using System.Net;

namespace Bulky.Http;

public class HttpContext(HttpListenerContext httpListenerContext, CancellationToken ct)
{
    public HttpRequest Request { get; private init; } = new(httpListenerContext.Request);
    public HttpResponse Response { get; private init; } = new(httpListenerContext.Response);
    public CancellationToken CancellationToken { get; set; } = ct;

    public async Task Respond()
    {
        foreach (var header in Response.Headers)
        {
            httpListenerContext.Response.Headers.Set(header.Key, header.Value);
        }

        httpListenerContext.Response.StatusCode = (int)Response.StatusCode;
        await httpListenerContext.Response.OutputStream.WriteAsync(Response.Output, CancellationToken);
    }
}