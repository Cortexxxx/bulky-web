using Bulky.Http;

namespace Bulky.Endpoints;

public class AuthEndpoints
{
    public Task LoginEndpoint(HttpContext context)
    {
        Console.WriteLine("Somebody trying to login");
        return Task.CompletedTask;
    }
}