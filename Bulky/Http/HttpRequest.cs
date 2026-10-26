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
        
        Method = request.HttpMethod switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PATCH" => HttpMethod.Patch,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            _ => throw new WebException("Unsupported method", HttpStatusCode.MethodNotAllowed)
        };
    }

    public Uri Url { get; private set; }
    public HttpMethod Method { get; set; }
    public Route? Route { get; set; }
}