using System.Text;
using Web.Http;

namespace Web.Endpoints;

public class PagesEndpoints
{
    public static async Task GetPage(HttpContext context, string relationPath)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.RenderAsync(relationPath)),
            ".html");
    }
}