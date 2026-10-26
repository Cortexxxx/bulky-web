using System.Net;

namespace Bulky.Middleware;

public static class ErrorMiddleware
{
    private const string NotFoundPath = "public/not_found.html";
    private const string ServerErrorPath = "public/server_error.html";
    public static void UseErrorMiddleware(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            try
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
            catch (WebException e) when ((int)e.StatusCode / 100 is 4 or 5)
            {
                
                context.Response.StatusCode = e.StatusCode;
                if (context.Request.Url.LocalPath.StartsWith("api/"))
                {
                    context.Response.WriteJson(new {Error = e.Error});
                }
                else
                {
                    switch (e.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            if (File.Exists(NotFoundPath))
                            {
                                context.Response.WriteBody(
                                    await File.ReadAllBytesAsync(NotFoundPath), ".html");
                            }
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                context.Response.StatusCode = HttpStatusCode.InternalServerError;
                
                if (File.Exists(ServerErrorPath))
                {
                    context.Response.WriteBody(
                        await File.ReadAllBytesAsync(ServerErrorPath), ".html");
                }
                
                Console.WriteLine($"Error is handled:\n {e}");
            }
        });
    }
}