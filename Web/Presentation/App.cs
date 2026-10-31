using System.Net;
using Web.Presentation.Http;

namespace Web.Presentation;

public class App(Middleware.Middleware middleware)
{
    public async Task HandleRequest(HttpListenerContext listenerContext, CancellationToken ct)
    {
        HttpContext context = default!;
        
        try
        {
            context = new HttpContext(listenerContext, ct);
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
                if (context != null)
                {
                    await context.Respond();
                }
                else
                {
                    throw new Exception("Context is null");
                }
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
    }
}