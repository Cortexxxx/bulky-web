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
        router.MapHtmlPage("", "home.html", Public);
        router.MapHtmlPage("home", "home.html", Public);
        router.MapHtmlPage("login", "login.html", GuestsOnly);

        router.MapHtmlPage("dashboard/account", "dashboard/account.html", AuthorizedOnly);
        router.MapHtmlPage("dashboard/workout", "dashboard/workout.html", AuthorizedOnly);
        router.MapHtmlPage("dashboard/history", "dashboard/history.html", AuthorizedOnly);
        router.MapHtmlPage("dashboard/analytics", "dashboard/analytics.html", AuthorizedOnly);

        router.MapHtmlPage("admin", "admin.html", AdminOnly);
    }
    
    public static void MapAuthApi(this Router router, AuthService authService,
        RegisterRequestValidator registerRequestValidator)
    {
        var authEndpoints = new AuthEndpoints(registerRequestValidator, authService);
        router.MapPost("api/auth/register", authEndpoints.RegisterUserAsync, GuestsOnly);
        router.MapPost("api/auth/login", authEndpoints.LoginUserAsync, GuestsOnly);
        router.MapGet("api/auth/me", authEndpoints.GetMeAsync, AuthorizedOnly);
        router.MapPost("api/auth/logout", authEndpoints.LogoutAsync, AuthorizedOnly);
        router.MapPost("api/auth/logoutall", authEndpoints.FullLogoutAsync, AuthorizedOnly);
        router.MapGet("api/auth/sessions", authEndpoints.GetSessionsAsync, AuthorizedOnly);
    }

    public static void MapMuscleGroupsApi(this Router router, MusclesService musclesService,
        MuscleGroupRequestValidator muscleGroupRequestValidator)
    {
        var musclesEndpoints = new MusclesEndpoints(musclesService, muscleGroupRequestValidator);

        router.MapGet("api/muscle_groups", context => 
            musclesEndpoints.GetAllMuscleGroupsAsync(false, context), AuthorizedOnly);
        router.MapGet("api/muscle_groups/{id}", context => 
            musclesEndpoints.GetMuscleGroupByIdAsync(false, context), AuthorizedOnly);
        router.MapGet("api/admin/muscle_groups", context => 
            musclesEndpoints.GetAllMuscleGroupsAsync(true, context), AdminOnly);
        router.MapGet("api/admin/muscle_groups/{id}", context => 
            musclesEndpoints.GetMuscleGroupByIdAsync(true, context), AdminOnly);

        router.MapPost("api/muscle_groups", musclesEndpoints.CreateMuscleGroupAsync, AdminOnly);
        router.MapPut("api/muscle_groups/{id}", musclesEndpoints.UpdateMuscleGroupAsync, AdminOnly);
        router.MapPost("api/muscle_groups/{id}/archive", musclesEndpoints.ArchiveMuscleGroupAsync, AdminOnly);
        router.MapPost("api/muscle_groups/{id}/unarchive", musclesEndpoints.UnarchiveMuscleGroupAsync, AdminOnly);
    }

    public static void MapExercisesApi(this Router router, ExercisesService exercisesService,
        ExerciseRequestValidator exerciseRequestValidator)
    {
        var exercisesEndpoints = new ExercisesEndpoints(exercisesService, exerciseRequestValidator);

        router.MapGet("api/exercises", context =>
            exercisesEndpoints.GetAllExercisesAsync(false, context), AuthorizedOnly);
        router.MapGet("api/exercises/{id}", context =>
            exercisesEndpoints.GetExerciseByIdAsync(false, context), AuthorizedOnly);
        router.MapGet("api/admin/exercises", context =>
            exercisesEndpoints.GetAllExercisesAsync(true, context), AdminOnly);
        router.MapGet("api/admin/exercises/{id}", context =>
            exercisesEndpoints.GetExerciseByIdAsync(true, context), AdminOnly);
        router.MapPost("api/exercises", exercisesEndpoints.CreateExerciseAsync, AdminOnly);
        router.MapPut("api/exercises/{id}", exercisesEndpoints.UpdateExerciseAsync, AdminOnly);
        router.MapPost("api/exercises/{id}/archive", exercisesEndpoints.ArchiveExerciseAsync, AdminOnly);
        router.MapPost("api/exercises/{id}/unarchive", exercisesEndpoints.UnarchiveExerciseAsync, AdminOnly);
    }

    public static void MapWorkoutsApi(this Router router, WorkoutsService workoutsService,
        WorkoutRequestValidator workoutRequestValidator)
    {
        var workoutEndpoints = new WorkoutsEndpoints(workoutsService, workoutRequestValidator);
        router.MapGet("api/workouts", workoutEndpoints.GetAllAsync, AuthorizedOnly);
        router.MapGet("api/workouts/current", workoutEndpoints.GetCurrentAsync, AuthorizedOnly);
        router.MapPost("api/workouts/", workoutEndpoints.CreateAsync, AuthorizedOnly);
        router.MapDelete("api/workouts/{id}", workoutEndpoints.DeleteAsync, AuthorizedOnly);
        router.MapPost("api/workouts/current/end", workoutEndpoints.EndCurrentAsync, AuthorizedOnly);
    }

    public static void MapWorkoutExercisesApi(this Router router, WorkoutExerciseService workoutExerciseService)
    {
        var workoutExerciseEndpoints = new WorkoutExercisesEndpoints(workoutExerciseService);
        router.MapPost("api/workouts/current/exercise", workoutExerciseEndpoints.CreateAsync, AuthorizedOnly);
    }
    
    private static void MapHtmlPage(this Router router, string route, string pageFilePath,
        AuthRequirements authRequirements)
    {
        router.MapPage(route, context => PagesEndpoints.GetPage(context, pageFilePath), authRequirements);
    }
}
