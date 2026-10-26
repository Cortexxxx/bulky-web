using System.Net;

namespace Bulky.Http;

public class HttpContext(HttpListenerContext httpListenerContext)
{
    public HttpRequest Request { get; private init; } = new(httpListenerContext.Request);
    public HttpResponse Response { get; private init; } = new(httpListenerContext.Response);
}