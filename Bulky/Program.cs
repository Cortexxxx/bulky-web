using System.Net;
using Bulky;
using Bulky.Data;
using Bulky.Endpoints;
using Bulky.Http;
using Bulky.Middleware;
using Bulky.Router;
using Bulky.Validators;
using Npgsql;

var httpListener = new HttpListener();
const string prefix = "http://localhost:8080/";

httpListener.Prefixes.Add(prefix);
Console.WriteLine(prefix);
httpListener.Start();

var cancellationTokenSource = new CancellationTokenSource();

var testService = new TestEndpoints();
var pagesEndpoints = new PagesEndpoints();

var db = new NpgsqlDataSourceBuilder(Environment.GetEnvironmentVariable("BULKY_DB_CONNECTION"));
var dataSource = db.Build();

var authEndpoints = new AuthEndpoints(new RegisterRequestValidator(), 
    new AuthService(new PostgresUserRepository(dataSource), new PostgresSessionRepository(dataSource)));

var router = new Router();
router.Map(HttpMethod.Get, "api/programs/{id}", testService.GetProgram, RouteKind.Api);
router.Map(HttpMethod.Get, "", pagesEndpoints.GetHomePage, RouteKind.Page);
router.Map(HttpMethod.Get, "home", pagesEndpoints.GetHomePage, RouteKind.Page);
router.Map(HttpMethod.Get, "login", pagesEndpoints.GetLoginPage, RouteKind.Page);
router.Map(HttpMethod.Post, "api/register", authEndpoints.RegisterEndpointAsync, RouteKind.Api);
router.Initialize();

var middleware = new Middleware();

middleware.UseErrorMiddleware();
middleware.UseStaticFiles();
middleware.UseRouting(router);
middleware.UseEndpoints();
middleware.UseNotFound();

middleware.Build();



while (httpListener.IsListening || cancellationTokenSource.IsCancellationRequested)
{
    var listenerContext = await httpListener.GetContextAsync();

    _ = Task.Run(async () =>
    {
        HttpContext context = null!;
        try
        {
            context = new HttpContext(listenerContext, cancellationTokenSource.Token);
            await (middleware.Pipeline?.Invoke(context) ?? Task.CompletedTask);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            try
            {
                await Response(context, listenerContext);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listenerContext.Response.OutputStream.Close();
            }
        }
    });


}

return;

async Task Response(HttpContext httpContext, HttpListenerContext httpListenerContext)
{
    foreach (var header in httpContext.Response.Headers)
    {
        httpListenerContext.Response.Headers.Set(header.Key, header.Value);
    }

    httpListenerContext.Response.StatusCode = (int)httpContext.Response.StatusCode;
    await httpListenerContext.Response.OutputStream.WriteAsync(httpContext.Response.Output);
}