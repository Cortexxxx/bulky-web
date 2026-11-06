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
    private static readonly AuthRequirements Public = new(AuthMode.Public);
    private static readonly AuthRequirements AuthorizedOnly = new(AuthMode.Required);
    private static readonly AuthRequirements AdminOnly = new(AuthMode.Required, Role.Admin);
    const RouteKind Api = RouteKind.Api;
    const RouteKind Page = RouteKind.Page;

    public static void Setup(this Middleware middleware, Router router, AuthService authService)
    {
        middleware.UseErrorMiddleware();
        middleware.UseStaticFiles();
        middleware.UseRouting(router);
        middleware.UseApiErrors();
        middleware.UseAuthentication(authService);
        middleware.UseAuthorization();
        middleware.UseEndpoints();
        middleware.UseNotFound();
    }

    public static void MapPages(this Router router)
    {
        router.Map(HttpMethod.Get, "", context =>
            PagesEndpoints.GetPage(context, "home.html"), Page, Public);
        router.Map(HttpMethod.Get, "home", context =>
            PagesEndpoints.GetPage(context, "home.html"), Page, Public);
        router.Map(HttpMethod.Get, "login", context =>
            PagesEndpoints.GetPage(context, "login.html"), Page, GuestsOnly);

        router.Map(HttpMethod.Get, "dashboard/account", context =>
            PagesEndpoints.GetPage(context, "dashboard/account.html"), Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/workout", context =>
            PagesEndpoints.GetPage(context, "dashboard/workout.html"), Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/history", context =>
            PagesEndpoints.GetPage(context, "dashboard/history.html"), Page, AuthorizedOnly);
        router.Map(HttpMethod.Get, "dashboard/analytics", context =>
            PagesEndpoints.GetPage(context, "dashboard/analytics.html"), Page, AuthorizedOnly);

        router.Map(HttpMethod.Get, "admin", context =>
            PagesEndpoints.GetPage(context, "admin.html"), Page, AdminOnly);
    }

    public static void MapAuthApi(this Router router, AuthService authService,
        RegisterRequestValidator registerRequestValidator)
    {
        var authEndpoints = new AuthEndpoints(registerRequestValidator, authService);
        router.Map(HttpMethod.Post, "api/auth/register", authEndpoints.RegisterUserAsync, Api, GuestsOnly);
        router.Map(HttpMethod.Post, "api/auth/login", authEndpoints.LoginUserAsync, Api, GuestsOnly);
        router.Map(HttpMethod.Get, "api/auth/me", authEndpoints.GetMeAsync, Api, AuthorizedOnly);
        router.Map(HttpMethod.Post, "api/auth/logout", authEndpoints.LogoutAsync, Api, AuthorizedOnly);
        router.Map(HttpMethod.Post, "api/auth/logoutall", authEndpoints.FullLogoutAsync, Api, AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/auth/sessions", authEndpoints.GetSessionsAsync, Api, AuthorizedOnly);
    }

    public static void MapMuscleGroupsApi(this Router router, MusclesService musclesService,
        MuscleGroupRequestValidator muscleGroupRequestValidator)
    {
        var musclesEndpoints = new MusclesEndpoints(musclesService, muscleGroupRequestValidator);
        
        router.Map(HttpMethod.Get, "api/muscle_groups",
            context => musclesEndpoints.GetAllMuscleGroupsAsync(false, context), Api, 
            AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/muscle_groups/{id}",
            context => musclesEndpoints.GetMuscleGroupByIdAsync(false, context), Api, 
            AuthorizedOnly);
        
        router.Map(HttpMethod.Get, "api/admin/muscle_groups",
            context => musclesEndpoints.GetAllMuscleGroupsAsync(true, context), Api, 
            AdminOnly);
        router.Map(HttpMethod.Get, "api/admin/muscle_groups/{id}",
            context => musclesEndpoints.GetMuscleGroupByIdAsync(true, context), Api, 
            AdminOnly);
        
        router.Map(HttpMethod.Post, "api/muscle_groups",
            musclesEndpoints.CreateMuscleGroupAsync, Api, AdminOnly);
        router.Map(HttpMethod.Put, "api/muscle_groups/{id}",
            musclesEndpoints.UpdateMuscleGroupAsync, Api, AdminOnly);
        router.Map(HttpMethod.Post, "api/muscle_groups/{id}/archive",
            musclesEndpoints.ArchiveMuscleGroupAsync, Api, AdminOnly);
        router.Map(HttpMethod.Post, "api/muscle_groups/{id}/unarchive",
            musclesEndpoints.UnarchiveMuscleGroupAsync, Api, AdminOnly);
    }
    
    public static void MapExercisesApi(this Router router, ExercisesService exercisesService,
        ExerciseRequestValidator exerciseRequestValidator)
    {
        var exercisesEndpoints = new ExercisesEndpoints(exercisesService, exerciseRequestValidator);

        router.Map(HttpMethod.Get, "api/exercises", context => 
            exercisesEndpoints.GetAllExercisesAsync(false, context), Api, AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/exercises/{id}", context => 
            exercisesEndpoints.GetExerciseByIdAsync(false, context), Api, AuthorizedOnly);

        router.Map(HttpMethod.Get, "api/admin/exercises", context => 
            exercisesEndpoints.GetAllExercisesAsync(true, context), Api, AdminOnly);
        router.Map(HttpMethod.Get, "api/admin/exercises/{id}", context => 
            exercisesEndpoints.GetExerciseByIdAsync(true, context), Api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises",
            exercisesEndpoints.CreateExerciseAsync, Api, AdminOnly);
        router.Map(HttpMethod.Put, "api/exercises/{id}",
            exercisesEndpoints.UpdateExerciseAsync, Api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises/{id}/archive",
            exercisesEndpoints.ArchiveExerciseAsync, Api, AdminOnly);
        router.Map(HttpMethod.Post, "api/exercises/{id}/unarchive",
            exercisesEndpoints.UnarchiveExerciseAsync, Api, AdminOnly);
    }

    public static void MapWorkoutsApi(this Router router, WorkoutsService workoutsService,
        WorkoutRequestValidator workoutRequestValidator)
    {
        var workoutEndpoints = new WorkoutsEndpoints(workoutsService, workoutRequestValidator);
        router.Map(HttpMethod.Get, "api/workouts", workoutEndpoints.GetAllAsync, 
            Api, AuthorizedOnly);
        router.Map(HttpMethod.Get, "api/workouts/current", workoutEndpoints.GetCurrentAsync, 
            Api, AuthorizedOnly);
        router.Map(HttpMethod.Post, "api/workouts/", workoutEndpoints.CreateAsync, 
            Api, AuthorizedOnly);
        router.Map(HttpMethod.Delete, "api/workouts/{id}", workoutEndpoints.DeleteAsync, 
            Api, AuthorizedOnly);
        router.Map(HttpMethod.Post, "api/workouts/current/end", workoutEndpoints.EndCurrentAsync, 
            Api, AuthorizedOnly);
    }
}