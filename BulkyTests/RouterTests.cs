using Bulky.Router;
using Bulky.Router.RouteResolutions;

namespace BulkyTests;

public class RouterTests
{
    private static HashSet<HttpMethod> GetTestAllowedSet()
    {
        return [HttpMethod.Get, HttpMethod.Post];
    }

    private static Router CreateTestRouter()
    {
        var router = new Router();
        router.Map(HttpMethod.Get,"api/programs/{id}", _ => Task.CompletedTask);
        router.Map(HttpMethod.Post, "api/programs", _ => Task.CompletedTask);
        router.Map(HttpMethod.Get, "api/programs", _ => Task.CompletedTask);
        router.Map(HttpMethod.Get, "api/programs/info", _ => Task.CompletedTask);
        router.Initialize();
        return router;
    }
    [Theory]
    [InlineData("GET", "/api/programs/5", 
        "api/programs/{id}", "5")]
    [InlineData("GET", "api/programs/0", 
        "api/programs/{id}", "0")]
    [InlineData("GET", "api/programs/0/", 
        "api/programs/{id}", "0")]
    [InlineData("POST", "api/programs", 
        "api/programs", null)]
    [InlineData("POST", "api/programs/", 
        "api/programs", null)]
    [InlineData("GET", "api/programs/info", 
        "api/programs/info", null)]
    public void Resolve_PathAndMethodMatches_ReturnsMatchedWithCorrectRouteAndParams(
        string methodString, string path, 
        string expectedTemplate, string? expectedId)
    {
        // Arrange
        var method = HttpMethod.Parse(methodString);
        var router = CreateTestRouter();
        
        // Act
        var resolution = router.Resolve(path, method);
        
        // Assert
        var matchedResolution = Assert.IsType<MatchedRouteResolution>(resolution);
        Assert.Equal(expectedTemplate, matchedResolution.Route.Template.Route);
        Assert.Equal(method, matchedResolution.Route.Method);
        matchedResolution.Parameters.TryGetValue("id", out var id);
        Assert.Equal(expectedId, id);
    }
    
    [Fact]
    public void Resolve_PathMatchesMethodNotMatches_ReturnsForbiddenWithCorrectAllowed()
    {
        // Arrange
        var method = HttpMethod.Parse("UPDATE");
        var router = CreateTestRouter();
        var allowed = GetTestAllowedSet();
        
        // Act
        var resolution = router.Resolve("api/programs/", method);
        
        // Assert
        var matchedResolution = Assert.IsType<ForbiddenMethodRouteResolution>(resolution);
        Assert.Equal(allowed.Count, matchedResolution.AllowedMethods.Count);
        foreach (var allowedMethod in matchedResolution.AllowedMethods)
        {
            Assert.Contains(allowedMethod, allowed);
        }
    }

    [Fact]
    public void Resolve_PathNotMatches_ReturnsUnknown()
    {
        // Arrange
        
        var router = CreateTestRouter();
        var method = HttpMethod.Get;
        var unknownPath = "/api/workouts";
        
        // Act

        var resolution = router.Resolve(unknownPath, method);
        
        // Assert

        Assert.IsType<UnknownRouteResolution>(resolution);
    }

    [Fact]
    public void Map_TryAddAlreadyExistedRoute_ThrowsException()
    {
        // Arrange
        var router = new Router();
        router.Map(HttpMethod.Get, "api/programs", _ => Task.CompletedTask);
        
        // Act
        
        // Assert
        Assert.Throws<InvalidOperationException>(() => 
                router.Map(HttpMethod.Get, "api/programs", _ => Task.CompletedTask));
    }
    
    [Fact]
    public void Map_TryAddRouteAfterInitialization_ThrowsException()
    {
        // Arrange
        var router = new Router();
        router.Map(HttpMethod.Get, "api/programs", _ => Task.CompletedTask);
        router.Initialize();
        // Act
        
        // Assert
        Assert.Throws<InvalidOperationException>(() => 
            router.Map(HttpMethod.Get, "api/workouts", _ => Task.CompletedTask));
    }
}