using System.Net;
using Bulky.Data;
using Bulky.Endpoints;
using Bulky.Http;
using Bulky.Middleware;
using Bulky.Router;
using Npgsql;

var httpListener = new HttpListener();
const string prefix = "http://localhost:8080/";

httpListener.Prefixes.Add(prefix);
Console.WriteLine(prefix);
httpListener.Start();

var testService = new TestEndpoints();
var pagesEndpoints = new PagesEndpoints();

var router = new Router();
router.Map(HttpMethod.Get, "api/programs/{id}", testService.GetProgram);
router.Map(HttpMethod.Get, "", pagesEndpoints.GetHomePage);
router.Map(HttpMethod.Get, "home", pagesEndpoints.GetHomePage);
router.Map(HttpMethod.Get, "login", pagesEndpoints.GetLoginPage);
router.Initialize();

var middleware = new Middleware();

middleware.UseErrorMiddleware();
middleware.UseStaticFiles();
middleware.UseRouting(router);
middleware.UseEndpoints();
middleware.UseNotFound();

middleware.Build();
var db = new NpgsqlDataSourceBuilder(Environment.GetEnvironmentVariable("BULKY_DB_CONNECTION"));
var dataSource = db.Build();
var usersRepository = new PostgresUserRepository(dataSource);
var sessionsRepository = new PostgresSessionRepository(dataSource);

while (httpListener.IsListening)
{
    var listenerContext = await httpListener.GetContextAsync();

    _ = Task.Run(async () =>
    {
        
        HttpContext context = null!;
        
        try
        {
            context = new HttpContext(listenerContext);
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