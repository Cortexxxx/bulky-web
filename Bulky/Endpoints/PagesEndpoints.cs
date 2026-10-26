using Bulky.Http;

namespace Bulky.Endpoints;

public class PagesEndpoints
{
    /// <summary>
    /// Endpoint: GET ""
    /// </summary>
    public async Task GetHomePage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("public/pages/home.html"), ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "login"
    /// </summary>
    public async Task GetLoginPage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("public/pages/login.html"), ".html");
    }
}