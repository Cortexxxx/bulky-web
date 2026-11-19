using System.Net;
using Application;
using Application.Services;
using Web.DTOs.Auth;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class AuthEndpoints(RegisterRequestValidator registerRequestValidator, AuthService authService, int sessionLifetimeMinutes)
{
    public async Task RegisterUserAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var normalizedRequest = await WebHelpers.ProceedRequestAsync(context, registerRequestValidator, 
            request => request with
            {
                Username = request.Username?.Trim() ?? string.Empty,
                Email = request.Email.ToLowerInvariant().Trim()
            }, ct);

        var userId = await authService.RegisterUserAsync(
            normalizedRequest.Username, normalizedRequest.Email, normalizedRequest.Password, ct);
        var (_, token) = await authService.CreateUserSessionAsync(userId, ct);

        context.Response.StatusCode = HttpStatusCode.Created;
        SetTokenCookie(context, token);
    }
    
    public async Task GetMeAsync(HttpContext context)
    {
        var user = context.Request.RequiredUserClaims;
        context.Response.WriteJson(new UserResponse(user.Id, user.Username, user.Email, user.Role, user.CreatedAt));
    }

    public async Task LoginUserAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var (email, password) = await context.Request.ReadJsonAsync<LoginRequest>(ct);

        var user = await authService.LoginUserAsync(email, password, ct);
        var (_, token) = await authService.CreateUserSessionAsync(user.Id, ct);
        
        SetTokenCookie(context, token);
    }

    public async Task LogoutAsync(HttpContext context)
    {
        var token = context.Request.GetCookie("token");
        
        if (token != null)
        {
            await authService.LogoutAsync(token.Value, context.CancellationToken);
        }

        FillLogoutResponse(context);
    }

    public async Task FullLogoutAsync(HttpContext context)
    {
        await authService.FullLogoutAsync(context.Request.RequiredUserClaims.Id, context.CancellationToken);
        FillLogoutResponse(context);
    }

    public async Task GetSessionsAsync(HttpContext context)
    {
        var sessions = await authService.GetUserSessionsAsync(context.Request.RequiredUserClaims.Id, 
            context.CancellationToken);
        
        context.Response.WriteJson(sessions.Select(s => s.ToResponse()).ToList());
    }

    private void FillLogoutResponse(HttpContext context)
    {
        SetTokenCookie(context, "");
        context.Response.StatusCode = HttpStatusCode.NoContent;
    }
    
    private void SetTokenCookie(HttpContext context, string token)
    {
        context.Response.SetCookie("token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            MaxAge = TimeSpan.FromMinutes(sessionLifetimeMinutes)
        });
    }
}
    