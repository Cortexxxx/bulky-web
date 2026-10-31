using Application.Services;
using Web.Presentation.Endpoints;
using Web.Presentation.Middleware;
using Web.Presentation.Router;
using Web.Presentation.Validators;

namespace Web.Presentation;

public static class SetupExtensions
{
    public static void Setup(this Middleware.Middleware middleware, Router.Router router, AuthService authService)
    {
        middleware.UseErrorMiddleware();
        middleware.UseApiErrors();
        middleware.UseStaticFiles();
        middleware.UseRouting(router);
        middleware.UseAuthentication(authService);
        middleware.UseEndpoints();
        middleware.UseNotFound();
    }

    public static void MapPages(this Router.Router router)
    {
        var pagesEndpoints = new PagesEndpoints();
        router.Map(HttpMethod.Get, "", pagesEndpoints.GetHomePage, RouteKind.Page);
        router.Map(HttpMethod.Get, "home", pagesEndpoints.GetHomePage, RouteKind.Page);
        router.Map(HttpMethod.Get, "login", pagesEndpoints.GetLoginPage, RouteKind.Page);
        router.Map(HttpMethod.Get, "account", pagesEndpoints.GetAccountPage, new AuthRequirements(true), RouteKind.Page);
    }

    public static void MapAuthApi(this Router.Router router, AuthService authService,
        RegisterRequestValidator registerRequestValidator)
    {
        var authEndpoints = new AuthEndpoints(registerRequestValidator, authService);
        router.Map(HttpMethod.Post, "api/auth/register", authEndpoints.RegisterEndpointAsync, RouteKind.Api);
        router.Map(HttpMethod.Post, "api/auth/login", authEndpoints.LoginEndpointAsync, RouteKind.Api);
        router.Map(HttpMethod.Get, "api/auth/me", authEndpoints.GetMeEndpointAsync, new AuthRequirements(true), RouteKind.Api);
    }
}