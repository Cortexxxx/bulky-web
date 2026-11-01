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
    
    /*
     /// <summary>
    /// Endpoint: GET ""
    /// </summary>
    public async Task GetHomePage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("www/pages/home.html"), ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "login"
    /// </summary>
    public async Task GetLoginPage(HttpContext context)
    {
        context.Response.WriteBody(await File.ReadAllBytesAsync("www/pages/login.html"), ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "dashboard/account"
    /// </summary>
    public async Task GetAccountPage(HttpContext context)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.Render("dashboard/account.html")), 
            ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "dashboard/workout"
    /// </summary>
    public async Task GetWorkoutPage(HttpContext context)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.Render("dashboard/workout.html")), 
            ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "dashboard/history"
    /// </summary>
    public async Task GetHistoryPage(HttpContext context)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.Render("dashboard/history.html")), 
            ".html");
    }
    
    /// <summary>
    /// Endpoint: GET "dashboard/analytics"
    /// </summary>
    public async Task GetAnalyticsPage(HttpContext context)
    {
        context.Response.WriteBody(Encoding.UTF8.GetBytes(await PageRenderer.Render("dashboard/analytics.html")), 
            ".html");
    }*/
}