using System.Net;
using System.Text;
using Bulky.Http;
using Bulky.Router;
using WebException = Bulky.Exceptions.WebException;

namespace Bulky.Middleware;

public static class ErrorMiddleware
{
    private static readonly Dictionary<string, string> ErrorPages = new()
    {
        {"not_found", "public/error_pages/not_found.html"},
        {"server_error", "public/error_pages/server_error.html"},
        {"unauthorized", "public/pages/login.html"}
    };
    private const string DefaultExceptionText = "Something went wrong";
    public static void UseErrorMiddleware(this Middleware middleware)
        {
            middleware.AddElement(async (context, next) =>
            {
                try
                {
                    await (next?.Invoke(context) ?? Task.CompletedTask);
                }
                catch (WebException e)
                {
                    context.Response.StatusCode = e.StatusCode;

                    var kind = WebHelpers.GetRouteKind(context.Request);
                    
                    switch (kind)
                    {
                        case RouteKind.Page:
                            ErrorPages.TryGetValue(e.Error, out var path);

                            context.Response.WriteBody(
                                path != null && File.Exists(path) 
                                    ? await File.ReadAllBytesAsync(path) 
                                    : Encoding.UTF8.GetBytes(DefaultExceptionText), ".html");
                            
                            break;
                        case RouteKind.Api:
                            context.Response.WriteJson(new {error = e.Error});
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(context));
                    }
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = HttpStatusCode.InternalServerError;
                    context.Response.WriteBody(ErrorPages.TryGetValue("server_error", out var path) 
                                               && File.Exists(path) 
                            ? await File.ReadAllBytesAsync(path) 
                            : Encoding.UTF8.GetBytes(DefaultExceptionText), ".html");
                    
                    Console.WriteLine($"Error is handled:\n {e}");
                }
            });
        }
    }