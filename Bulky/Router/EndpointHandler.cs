using Bulky.Http;

namespace Bulky.Router;

public delegate Task EndpointHandler(HttpContext context);