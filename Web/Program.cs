using System.Net;
using Application.Services;
using Npgsql;
using Persistence;
using Persistence.Data;
using Persistence.Redis;
using StackExchange.Redis;
using Web.Pipeline;
using Web.Routing;
using Web.Validators;
using Web;
using Web.Http;

var httpListener = new HttpListener();

var prefix = WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.Prefix);
httpListener.Prefixes.Add(prefix);
httpListener.Start();
Console.WriteLine(prefix);
var appCts = new CancellationTokenSource();

// Data

var postgres = new NpgsqlDataSourceBuilder(
    WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.PostgresConnection));
var redis = await ConnectionMultiplexer.ConnectAsync(
    WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.RedisConnection));

var postgresDataSource = postgres.Build();
var redisDataSource = redis.GetDatabase();

// Services

var authService = new AuthService(new PostgresUserRepository(postgresDataSource), 
    new PostgresSessionRepository(postgresDataSource), new PasswordHasher());
var musclesService = new MusclesService(new CachedMuscleGroupsRepository(redisDataSource, 
    new PostgresMuscleGroupsRepository(postgresDataSource)));
var exercisesService = new ExercisesService(new CachedExercisesRepository(redisDataSource, 
    new PostgresExercisesRepository(postgresDataSource)));
var workoutsService = new WorkoutsService(new PostgresWorkoutsRepository(postgresDataSource));
var workoutExercisesService = new WorkoutExerciseService(new PostgresWorkoutExerciseRepository(postgresDataSource));
var analyticsService = new WorkoutsAnalyticsService(new PostgresAnalyticsQueries(postgresDataSource));
var guidesService = new GuideService(exercisesService, musclesService);

// Routing

var router = new Router();
router.MapPages();
router.MapAuthApi(authService, new RegisterRequestValidator());
router.MapMuscleGroupsApi(musclesService, new MuscleGroupRequestValidator());
router.MapExercisesApi(exercisesService, new ExerciseRequestValidator());
router.MapWorkoutsApi(workoutsService, new WorkoutRequestValidator());
router.MapWorkoutExercisesApi(workoutExercisesService, new WorkoutSetRequestValidator());
router.MapWorkoutAnalyticsApi(analyticsService);
router.MapGuidesApi(guidesService);
router.Initialize();

// Middleware

var middleware = new Middleware();
middleware.Setup(router, authService);
middleware.Build();

// Startup

await authService.DeleteExpiredSessionsAsync(appCts.Token);

// App cycle

var app = new App(middleware);

Console.CancelKeyPress += async (_, _) =>
{
    await appCts.CancelAsync();
};

while (httpListener.IsListening || appCts.IsCancellationRequested)
{
    var listenerContext = await httpListener.GetContextAsync();
    _ = Task.Run(() => app.HandleRequest(listenerContext, appCts.Token));
}
