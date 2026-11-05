using System.Net;
using Application.Services;
using Npgsql;
using Persistence;
using Persistence.Data;
using Web.Pipeline;
using Web.Routing;
using Web.Validators;
using Web;

var httpListener = new HttpListener();
var prefix = Environment.GetEnvironmentVariable(EnvironmentVariables.Prefix)
             ?? throw new Exception($"Environment variable '{EnvironmentVariables.Prefix}' is not set");
var db = new NpgsqlDataSourceBuilder(Environment.GetEnvironmentVariable(EnvironmentVariables.DbConnection)
                                     ?? throw new Exception($"Environment variable '{EnvironmentVariables.DbConnection}' is not set"));
Console.WriteLine(prefix);

httpListener.Prefixes.Add(prefix);
httpListener.Start();


// Dependencies

var appCts = new CancellationTokenSource();
var dataSource = db.Build();
var authService = new AuthService(new PostgresUserRepository(dataSource), 
    new PostgresSessionRepository(dataSource), new PasswordHasher());
var musclesService = new MusclesService(new PostgresMuscleGroupsRepository(dataSource));
var exercisesService = new ExercisesService(new PostgresExercisesRepository(dataSource));

// Routing

var router = new Router();
router.MapPages();
router.MapAuthApi(authService, new RegisterRequestValidator());
router.MapMuscleGroupsApi(musclesService, new MuscleGroupRequestValidator());
router.MapExercisesApi(exercisesService, new ExerciseRequestValidator());
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
