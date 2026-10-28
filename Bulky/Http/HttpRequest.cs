using System.Net;
using Bulky.Router;

namespace Bulky.Http;

public class HttpRequest
{
    public HttpRequest(HttpListenerRequest request)
    {
        if (request.Url == null)
        {
            throw new WebException("Url is null", HttpStatusCode.BadRequest);
        }

        Url = request.Url;
        Method = HttpMethod.Parse(request.HttpMethod);
    }

    public Uri Url { get; private set; }
    public HttpMethod Method { get; set; }
    public IReadOnlyDictionary<string, string> RouteParameters { get; set; }
    public Route? Route { get; set; }
}