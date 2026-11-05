using System.Net;

namespace Web.Pipeline;

public static class StaticFilesMiddleware
{
    public static void UseStaticFiles(this Middleware middleware)
    {
        middleware.AddElement(async (context, next) =>
        {
            var url = context.Request.Url;

            var localPath = url.LocalPath;
            var path = Path.Combine(FilePaths.Public, localPath.Trim('/', '\\'));
            var absolutePath = Path.GetFullPath(path);
            var publicPath = Path.GetFullPath(FilePaths.Public) + Path.DirectorySeparatorChar;
            
            if (File.Exists(absolutePath) && absolutePath.StartsWith(publicPath))
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