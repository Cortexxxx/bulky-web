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

    private List<string> _programs =
    [
        "Pull-Push-Legs",
        "Top-Down-Top-Down",
        "Chest-Back-Legs",
        "Full-body"
    ];
    
    public Task GetProgram(HttpContext context)
    {
        var parameters = context.Request.RouteParameters;
        string? program = null;
        if (parameters != null && parameters.TryGetValue("id", out var stringId))
        {
            var tryParse = int.TryParse(stringId, out var id);
            
            if (!tryParse || _programs.Count <= id)
            {
                throw new WebException("Not found", HttpStatusCode.NotFound);
            }

            program = _programs[id];
        }
        else
        {
            throw new WebException("Not found", HttpStatusCode.NotFound);
        }
        
        var obj = new { Title = program };
        
        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.WriteJson(obj);
        return Task.CompletedTask;
    }
}