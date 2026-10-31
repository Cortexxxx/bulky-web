using System.Net;
using Bulky;
using Bulky.Data;
using Bulky.Http;
using Bulky.Middleware;
using Bulky.Router;
using Bulky.Services;
using Bulky.Validators;
using Npgsql;

var httpListener = new HttpListener();
var prefix = Environment.GetEnvironmentVariable("PREFIX") 
             ?? throw new Exception("Environment variable 'PREFIX' is not set");
Console.WriteLine(prefix);

httpListener.Prefixes.Add(prefix);
httpListener.Start();


var appCts = new CancellationTokenSource();

var db = new NpgsqlDataSourceBuilder(Environment.GetEnvironmentVariable("BULKY_DB_CONNECTION"));
var dataSource = db.Build();

var authService = new AuthService(new PostgresUserRepository(dataSource), new PostgresSessionRepository(dataSource));

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