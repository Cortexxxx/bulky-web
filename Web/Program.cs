using System.Net;
using Application.Services;
using Npgsql;
using Web.Persistence;
using Web.Persistence.Data;
using Web.Presentation.Middleware;
using Web.Presentation.Router;
using Web.Presentation.Validators;
using Web.Presentation;

var httpListener = new HttpListener();
var prefix = Environment.GetEnvironmentVariable("PREFIX") 
             ?? throw new Exception("Environment variable 'PREFIX' is not set");
var db = new NpgsqlDataSourceBuilder(Environment.GetEnvironmentVariable("BULKY_DB_CONNECTION")
                                     ?? throw new Exception("Environment variable 'BULKY_DB_CONNECTION' is not set"));
Console.WriteLine(prefix);

httpListener.Prefixes.Add(prefix);
httpListener.Start();

var appCts = new CancellationTokenSource();
var dataSource = db.Build();
var authService = new AuthService(new PostgresUserRepository(dataSource), 
    new PostgresSessionRepository(dataSource), new PasswordHasher());

// Routing

var router = new Router();
router.MapPages();
router.MapAuthApi(authService, new RegisterRequestValidator());
router.Initialize();

// Middleware

var middleware = new Middleware();
middleware.Setup(router, authService);
middleware.Build();

// App cycle

var app = new App(middleware);

while (httpListener.IsListening || appCts.IsCancellationRequested)
{
    var listenerContext = await httpListener.GetContextAsync();
    _ = Task.Run(() => app.HandleRequest(listenerContext, appCts.Token));
}