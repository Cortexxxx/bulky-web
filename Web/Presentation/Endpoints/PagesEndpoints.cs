using System.Text;
using Web.Presentation.Http;

namespace Web.Presentation.Endpoints;

public class PagesEndpoints
{
    public async Task GetPage(HttpContext context, string relationPath)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.RenderAsync(relationPath)),
            ".html");
    }
}