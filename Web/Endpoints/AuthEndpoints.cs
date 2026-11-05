using System.Net;
using Application.Services;
using Web.DTOs.Auth;
using Web.Exceptions;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class AuthEndpoints(RegisterRequestValidator registerRequestValidator, AuthService authService)
{
    public async Task RegisterUserAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var registerRequest = await context.Request.ReadJsonAsync<RegisterRequest>(ct);
        var normalizedRequest = registerRequest with { Email = registerRequest.Email.ToLowerInvariant().Trim() };
        await WebHelpers.ValidateRequestAsync(registerRequestValidator, normalizedRequest,
            "Incorrect register request", ct);

        var userId = await authService.RegisterUserAsync(normalizedRequest.Email, normalizedRequest.Password, ct);
        var (_, token) = await authService.CreateUserSessionAsync(userId, ct);

        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.SetContentLength(0);
        SetTokenCookie(context, token);
    }
    
    public async Task GetMeAsync(HttpContext context)
    {
        var user = context.Request.UserClaims;
        if (user == null) ExceptionsHelpers.ThrowUnauthorized();
        context.Response.WriteJson(user);
    }

    public async Task LoginUserAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var (email, password) = await context.Request.ReadJsonAsync<LoginRequest>(ct);

        var user = await authService.LoginUserAsync(email, password, ct);
        var (_, token) = await authService.CreateUserSessionAsync(user.Id, ct);
        
        context.Response.SetContentLength(0);
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
        if (context.Request.UserClaims == null) ExceptionsHelpers.ThrowUnauthorized();
        
        await authService.FullLogoutAsync(context.Request.UserClaims.Id, context.CancellationToken);
        FillLogoutResponse(context);
    }

    public async Task GetSessionsAsync(HttpContext context)
    {
        if (context.Request.UserClaims == null) ExceptionsHelpers.ThrowUnauthorized();

        var sessions = await authService.GetUserSessionsAsync(context.Request.UserClaims.Id, context.CancellationToken);
        
        context.Response.WriteJson(sessions.Select(s => s.ToResponse()).ToList());
    }

    private static void FillLogoutResponse(HttpContext context)
    {
        SetTokenCookie(context, "");
        context.Response.StatusCode = HttpStatusCode.NoContent;
        context.Response.SetContentLength(0);
    }
    
    private static void SetTokenCookie(HttpContext context, string token)
    {
        context.Response.SetCookie("token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            MaxAge = TimeSpan.FromMinutes(5)
        });
    }
}