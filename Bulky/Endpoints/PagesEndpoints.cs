using Bulky.Exceptions;
using Bulky.Http;

namespace Bulky.Endpoints;

public class PagesEndpoints
{
    /// <summary>
    /// Endpoint: GET ""
    /// </summary>
    public async Task GetHomePage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("pages/home.html"), ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "login"
    /// </summary>
    public async Task GetLoginPage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("pages/login.html"), ".html");
    }

    public async Task GetPage(HttpContext context)
    {
        try
        {
            context.Response.WriteBody(await File.ReadAllBytesAsync($"pages/{context.Request.Url.LocalPath}.html"), ".html");
        }
        catch (Exception e)
        {
            ExceptionsHelpers.ThrowNotFound("Not found page by path");
            throw;
        }
    }
}