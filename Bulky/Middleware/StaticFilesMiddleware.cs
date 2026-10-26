using System.Net;

namespace Bulky.Middleware;

public static class StaticFilesMiddleware
{
    public static void UseStaticFiles(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            var url = context.Request.Url;

            var localPath = url.LocalPath;
            var path = Path.Combine("public", localPath.Trim('/', '\\'));
            var absolutePath = Path.GetFullPath(path);
            var publicPath = Path.GetFullPath("public") + Path.DirectorySeparatorChar;
            
            if (File.Exists(path) && absolutePath.StartsWith(publicPath))
            {
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.WriteBody(
                    await File.ReadAllBytesAsync(path), Path.GetExtension(path));
            }
            else
            {
                await (next?.Invoke(context) ?? Task.CompletedTask);
            }
        });
    }
}