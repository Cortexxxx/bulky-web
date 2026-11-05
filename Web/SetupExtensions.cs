using Application.Services;
using Domain.Models.Auth;
using Web.Endpoints;
using Web.Pipeline;
using Web.Routing;
using Web.Validators;

namespace Web;

public static class SetupExtensions
{
    private static readonly AuthRequirements GuestsOnly = new(AuthMode.GuestsOnly);
    private static readonly AuthRequirements AuthorizedOnly = new(AuthMode.Required);
    private static readonly AuthRequirements AdminOnly = new(AuthMode.Required, Role.Admin);

    public static void Setup(this Middleware middleware, Router router, AuthService authService)
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

    public static void MapPages(this Router router)
    {
        var pagesEndpoints = new PagesEndpoints();

        router.Map(HttpMethod.Get, "", context =>
            pagesEndpoints.GetPage(context, "home.html"), RouteKind.Page, GuestsOnly);
        router.Map(HttpMethod.Get, "home", context =>
            pagesEndpoints.GetPage(context, "home.html"), RouteKind.Page, GuestsOnly);
        router.Map(HttpMethod.Get, "login", context =>
            pagesEndpoints.GetPage(context, "login.html"), RouteKind.Page, GuestsOnly);

        router.Map(HttpMethod.Get, "dashboard/account", context =>
            pagesEndpoints.GetPage(context, "dashboard/account.html"), RouteKind.Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/workout", context =>
            pagesEndpoints.GetPage(context, "dashboard/workout.html"), RouteKind.Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/history", context =>
            pagesEndpoints.GetPage(context, "dashboard/history.html"), RouteKind.Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/analytics", context =>
            pagesEndpoints.GetPage(context, "dashboard/analytics.html"), RouteKind.Page, AuthorizedOnly);

        router.Map(HttpMethod.Get, "admin", context =>
            pagesEndpoints.GetPage(context, "admin.html"), RouteKind.Page, AdminOnly);
    }

    public static void MapAuthApi(this Router router, AuthService authService,
        RegisterRequestValidator registerRequestValidator)
    {
        var authEndpoints = new AuthEndpoints(registerRequestValidator, authService);
        router.Map(HttpMethod.Post, "api/auth/register", authEndpoints.RegisterUserAsync, RouteKind.Api);
        router.Map(HttpMethod.Post, "api/auth/login", authEndpoints.LoginUserAsync, RouteKind.Api);
        router.Map(HttpMethod.Get, "api/auth/me", authEndpoints.GetMeAsync, RouteKind.Api, AuthorizedOnly);
    }

    public static void MapMuscleGroupsApi(this Router router, MusclesService musclesService,
        MuscleGroupRequestValidator muscleGroupRequestValidator)
    {
        var musclesEndpoints = new MusclesEndpoints(musclesService, muscleGroupRequestValidator);
        const RouteKind kind = RouteKind.Api;
        
        router.Map(HttpMethod.Get, "api/muscle_groups",
            context => musclesEndpoints.GetAllMuscleGroupsAsync(false, context), kind, 
            AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/muscle_groups/{id}",
            context => musclesEndpoints.GetMuscleGroupByIdAsync(false, context), kind, 
            AuthorizedOnly);
        
        router.Map(HttpMethod.Get, "api/admin/muscle_groups",
            context => musclesEndpoints.GetAllMuscleGroupsAsync(true, context), kind, 
            AdminOnly);
        router.Map(HttpMethod.Get, "api/admin/muscle_groups/{id}",
            context => musclesEndpoints.GetMuscleGroupByIdAsync(true, context), kind, 
            AdminOnly);
        
        router.Map(HttpMethod.Post, "api/muscle_groups",
            musclesEndpoints.CreateMuscleGroupAsync, kind, AdminOnly);
        router.Map(HttpMethod.Put, "api/muscle_groups/{id}",
            musclesEndpoints.UpdateMuscleGroupAsync, kind, AdminOnly);
        router.Map(HttpMethod.Post, "api/muscle_groups/{id}/archive",
            musclesEndpoints.ArchiveMuscleGroupAsync, kind, AdminOnly);
        router.Map(HttpMethod.Post, "api/muscle_groups/{id}/unarchive",
            musclesEndpoints.UnarchiveMuscleGroupAsync, kind, AdminOnly);
    }
    
    public static void MapExercisesApi(this Router router, ExercisesService exercisesService,
        ExerciseRequestValidator exerciseRequestValidator)
    {
        const RouteKind api = RouteKind.Api;
        var exercisesEndpoints = new ExercisesEndpoints(exercisesService, exerciseRequestValidator);

        router.Map(HttpMethod.Get, "api/exercises", context => 
            exercisesEndpoints.GetAllExercisesAsync(false, context), api, AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/exercises/{id}", context => 
            exercisesEndpoints.GetExerciseByIdAsync(false, context), api, AuthorizedOnly);

        router.Map(HttpMethod.Get, "api/admin/exercises", context => 
            exercisesEndpoints.GetAllExercisesAsync(true, context), api, AdminOnly);
        router.Map(HttpMethod.Get, "api/admin/exercises/{id}", context => 
            exercisesEndpoints.GetExerciseByIdAsync(true, context), api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises",
            exercisesEndpoints.CreateExerciseAsync, api, AdminOnly);
        router.Map(HttpMethod.Put, "api/exercises/{id}",
            exercisesEndpoints.UpdateExerciseAsync, api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises/{id}/archive",
            exercisesEndpoints.ArchiveExerciseAsync, api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises/{id}/unarchive",
            exercisesEndpoints.UnarchiveExerciseAsync, api, AdminOnly);
    }
}