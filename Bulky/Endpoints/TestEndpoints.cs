using System.Net;
using Bulky.Http;

namespace Bulky.Endpoints;

public class TestEndpoints
{
    public Task GetBiceps(HttpContext context)
    {
        var obj = new { Title = "Biceps is great muscle" };

        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.WriteJson(obj);
        return Task.CompletedTask;
    }
}