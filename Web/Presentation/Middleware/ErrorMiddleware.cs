using System.Net;
using System.Text;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Router;
using WebException = Web.Presentation.Exceptions.WebException;

namespace Web.Presentation.Middleware;

public static class ErrorMiddleware
{
    private static readonly Dictionary<string, string> ErrorPages = new()
    {
        {"not_found", "www/pages/error-pages/not_found.html"},
        {"server_error", "www/pages/error-pages/server_error.html"},
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
                        case RouteKind.Page when context.Response.StatusCode == HttpStatusCode.Unauthorized:
                            context.Response.StatusCode = HttpStatusCode.Redirect;
                            context.Response.SetContentLength(0);
                            context.Response.Headers["Location"] = "/login";
                            break;
                        case RouteKind.Page:
                            ErrorPages.TryGetValue(e.Error, out var path);

                            context.Response.WriteBody(
                                path != null && File.Exists(path) 
                                    ? await File.ReadAllBytesAsync(path) 
                                    : Encoding.UTF8.GetBytes(DefaultExceptionText), ".html");
                            
                            break;
                        case RouteKind.Api:
                            context.Response.WriteJson(new { error = (string[]) [e.Error] });
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