using Bulky.Http;

namespace Bulky.Middleware;

public delegate Task ChainRest(HttpContext context);
public delegate Task MiddlewareElement(HttpContext context, ChainRest? next);

public class Middleware
{
    public ChainRest? Pipeline;
    private readonly List<MiddlewareElement> _elements = [];

    public void AddElement(MiddlewareElement element)
    {
        _elements.Add(element);
    }

    public void Build()
    {

        for (var i = _elements.Count - 1; i >= 0; i--)
        {
            var iCopy = i;
            
            if (Pipeline == null)
            {
                Pipeline = httpContext => _elements[iCopy](httpContext, null);
            }
            else
            {
                var pipeline = Pipeline;
                Pipeline = httpContext => _elements[iCopy](httpContext, pipeline);
            }
        }   
    }
}