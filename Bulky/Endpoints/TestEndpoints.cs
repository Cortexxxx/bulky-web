using System.Net;
using Bulky.Http;
using Bulky.Router;

namespace Bulky.Endpoints;

public class TestEndpoints
{
    private readonly List<string> _programs =
    [
        "Pull-Push-Legs",
        "Top-Down-Top-Down",
        "Chest-Back-Legs",
        "Full-body"
    ];
    
    /// <summary>
    /// Endpoint: api/workouts/{id}
    /// </summary>
    public Task GetProgram(HttpContext context)
    {
        if (!Router.Router.TryGetIntParameter(context.Request.RouteParameters, "id", out var id))
        {
            WebHelpers.ThrowNotFound();
        }
        else
        {
            if (_programs.Count <= id.Value || id.Value < 0)
            {
                WebHelpers.ThrowNotFound();
            }
            
            var program = _programs[id.Value];
            var obj = new { Title = program };
        
            context.Response.WriteJson(obj);
        }
        
        return Task.CompletedTask;
    }
}