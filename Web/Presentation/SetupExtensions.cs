using Application.Services;
using Domain.Models.Auth;
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
        middleware.UseAuthorization();
        middleware.UseEndpoints();
        middleware.UseNotFound();
    }

    public static void MapPages(this Router.Router router)
    {
        var pagesEndpoints = new PagesEndpoints();
        var guestsOnly = new AuthRequirements(AuthMode.GuestsOnly);
        var authorizedOnly = new AuthRequirements(AuthMode.Required);
        var adminOnly = authorizedOnly with {RequiredRole = Role.Admin};
        
        router.Map(HttpMethod.Get, "", context =>
            pagesEndpoints.GetPage(context, "home.html"), RouteKind.Page, guestsOnly);
        router.Map(HttpMethod.Get, "home", context => 
            pagesEndpoints.GetPage(context, "home.html"), RouteKind.Page, guestsOnly);
        router.Map(HttpMethod.Get, "login", context => 
            pagesEndpoints.GetPage(context, "login.html"), RouteKind.Page, guestsOnly);
        
        router.Map(HttpMethod.Get, "dashboard/account", context => 
            pagesEndpoints.GetPage(context, "dashboard/account.html"), RouteKind.Page, authorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/workout", context => 
            pagesEndpoints.GetPage(context, "dashboard/workout.html"), RouteKind.Page, authorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/history", context => 
            pagesEndpoints.GetPage(context, "dashboard/history.html"), RouteKind.Page, authorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/analytics", context => 
            pagesEndpoints.GetPage(context, "dashboard/analytics.html"), RouteKind.Page, authorizedOnly);
        
        router.Map(HttpMethod.Get, "admin", context => 
            pagesEndpoints.GetPage(context, "admin/admin.html"), RouteKind.Page, adminOnly);
    }

    public static void MapAuthApi(this Router.Router router, AuthService authService,
        RegisterRequestValidator registerRequestValidator)
    {
        var authEndpoints = new AuthEndpoints(registerRequestValidator, authService);
        router.Map(HttpMethod.Post, "api/auth/register", authEndpoints.RegisterEndpointAsync, RouteKind.Api);
        router.Map(HttpMethod.Post, "api/auth/login", authEndpoints.LoginEndpointAsync, RouteKind.Api);
        router.Map(HttpMethod.Get, "api/auth/me", authEndpoints.GetMeEndpointAsync, RouteKind.Api, new AuthRequirements(AuthMode.Required));
    }

    public static void MapMuscleGroupsApi(this Router.Router router, AdminService adminService,
        MuscleGroupRequestValidator muscleGroupRequestValidator)
    {
        var adminEndpoints = new AdminEndpoints(adminService, muscleGroupRequestValidator);
        const RouteKind kind = RouteKind.Api;
        var adminRequirements = new AuthRequirements(AuthMode.Required, Role.Admin);
        
        router.Map(HttpMethod.Get, "api/admin/muscle_groups", 
            adminEndpoints.GetAllMuscleGroupsEndpoint, kind, new AuthRequirements(AuthMode.Required));
        router.Map(HttpMethod.Post, "api/admin/muscle_groups", 
            adminEndpoints.CreateMuscleGroupEndpoint, kind, adminRequirements);
        router.Map(HttpMethod.Put, "api/admin/muscle_groups/{id}", 
            adminEndpoints.UpdateMuscleGroupEndpoint, kind, adminRequirements);
        router.Map(HttpMethod.Post, "api/admin/muscle_groups/{id}/archive", 
            adminEndpoints.ArchiveMuscleGroupEndpoint, kind, adminRequirements);
        router.Map(HttpMethod.Post, "api/admin/muscle_groups/{id}/unarchive", 
            adminEndpoints.ArchiveMuscleGroupEndpoint, kind, adminRequirements);
    }
}