using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Application.Services;
using Microsoft.Extensions.Configuration;
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

// Configuration

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var validationOptions = config.GetValidationOptions();
var exerciseHintOptions = config.GetExerciseHintOptions();
var cacheTtl = config.GetCacheTtl();
var sessionLifetime = config.GetSessionLifetime();
var httpListener = new HttpListener();

// Initialization

var prefix = WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.Prefix);
httpListener.Prefixes.Add(prefix);
httpListener.Start();
Console.WriteLine(prefix);
var appCts = new CancellationTokenSource();

// Persistance

var postgres = new NpgsqlDataSourceBuilder(
    WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.PostgresConnection));
var redis = await ConnectionMultiplexer.ConnectAsync(
    WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.RedisConnection));
var s3Client = new AmazonS3Client(new AmazonS3Config
{
    ServiceURL = WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.AwsServiceUrl),
    AuthenticationRegion = WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.AwsAuthRegion),
    ForcePathStyle = true,
    DefaultAWSCredentials = new BasicAWSCredentials(
        WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.AwsAccessKeyId), 
        WebHelpers.GetEnviromentVariableOrThrow(EnvironmentVariables.AwsSecretAccessKey))
});

var postgresDataSource = postgres.Build();
var redisDataSource = redis.GetDatabase();

// Services

var authService = new AuthService(new PostgresUserRepository(postgresDataSource), 
    new PostgresSessionRepository(postgresDataSource), new PasswordHasher(), sessionLifetime);
var musclesService = new MusclesService(new CachedMuscleGroupsRepository(redisDataSource, 
    new PostgresMuscleGroupsRepository(postgresDataSource), cacheTtl));
var exercisesService = new ExercisesService(new CachedExercisesRepository(redisDataSource, 
    new PostgresExercisesRepository(postgresDataSource), cacheTtl));
var workoutsService = new WorkoutsService(new PostgresWorkoutsRepository(postgresDataSource), new RedisCacheInvalidator(redisDataSource));
var workoutExercisesService = new WorkoutExerciseService(new PostgresWorkoutExerciseRepository(postgresDataSource), exerciseHintOptions);
var analyticsService = new WorkoutsAnalyticsService(new CachedAnalyticsQueries(
    new PostgresAnalyticsQueries(postgresDataSource), redisDataSource, cacheTtl));
var guidesService = new GuideService(exercisesService, musclesService);
var imageUrlProvider = new ImageUrlProvider(s3Client);

// Routing

var router = new Router();
router.MapPages();
router.MapAuthApi(authService, new RegisterRequestValidator(validationOptions), sessionLifetime);
router.MapMuscleGroupsApi(musclesService, new MuscleGroupRequestValidator(validationOptions));
router.MapExercisesApi(exercisesService, new ExerciseRequestValidator());
router.MapImagesApi(imageUrlProvider);
router.MapWorkoutsApi(workoutsService, new WorkoutRequestValidator());
router.MapWorkoutExercisesApi(workoutExercisesService, new WorkoutSetRequestValidator(validationOptions));
router.MapWorkoutAnalyticsApi(analyticsService);
router.MapGuidesApi(guidesService, imageUrlProvider);
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
