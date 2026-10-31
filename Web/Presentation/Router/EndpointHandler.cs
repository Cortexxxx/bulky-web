using Web.Presentation.Http;

namespace Web.Presentation.Router;

public delegate Task EndpointHandler(HttpContext context);