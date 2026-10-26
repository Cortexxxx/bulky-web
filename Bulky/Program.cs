using System.Net;
using Bulky.Endpoints;
using Bulky.Http;
using Bulky.Middleware;
using Bulky.Router;

var httpListener = new HttpListener();
const string prefix = "http://localhost:8080/";

httpListener.Prefixes.Add(prefix);
Console.WriteLine(prefix);
httpListener.Start();

var testService = new TestEndpoints();

List<Route> endpoints =
[
    new Route(HttpMethod.Get, new RouteTemplate("api/programs/{id}"), new AuthRequirements(false), testService.GetProgram)
];

var router = new Router(endpoints.ToArray());

var middleware = new Middleware();

middleware.UseErrorMiddleware();
middleware.UseStaticFiles();

middleware.UseRouting(router);
middleware.UseEndpoints();

middleware.UseNotFound();

middleware.Build();

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