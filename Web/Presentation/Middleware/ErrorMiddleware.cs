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
        {"forbidden", "www/pages/error-pages/forbidden.html"},
    };
    private const string DefaultExceptionText = "Something went wrong";
    public static void UseErrorMiddleware(this Middleware middleware)
        {
            middleware.AddElement(async (context, next) =>
            {
                var response = context.Response;
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
                        case RouteKind.Page when e.Error == "already_authenticated" && 
                                                 context.Request.Route?.AuthRequirements.AuthMode == AuthMode.GuestsOnly:
                            response.StatusCode = HttpStatusCode.Redirect;
                            response.Headers["Location"] = PathConstants.DashboardWorkoutPagePath;
                            response.SetContentLength(0);
                            break;
                        case RouteKind.Page when context.Response.StatusCode == HttpStatusCode.Unauthorized:
                            response.StatusCode = HttpStatusCode.Redirect;
                            response.Headers["Location"] = PathConstants.AuthorizationPagePath;
                            response.SetContentLength(0);
                            break;
                        case RouteKind.Page:
                            ErrorPages.TryGetValue(e.Error, out var path);

                            response.WriteBody(
                                path != null && File.Exists(path) 
                                    ? await File.ReadAllBytesAsync(path) 
                                    : Encoding.UTF8.GetBytes(DefaultExceptionText), ".html");
                            break;
                        case RouteKind.Api:
                            response.WriteJson(new { error = (string[]) [e.Error] });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(context));
                    }
                }
                catch (Exception e)
                {
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.WriteBody(ErrorPages.TryGetValue("server_error", out var path) 
                                       && File.Exists(path) 
                            ? await File.ReadAllBytesAsync(path) 
                            : Encoding.UTF8.GetBytes(DefaultExceptionText), ".html");
                    
                    Console.WriteLine($"Error is handled:\n {e}");
                }
            });
        }
    }