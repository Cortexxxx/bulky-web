using System.Net;
using Application.Services;
using Web.Presentation.DTOs;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Validators;
using ValidationException = FluentValidation.ValidationException;

namespace Web.Presentation.Endpoints;

public class AuthEndpoints(RegisterRequestValidator registerRequestValidator, AuthService authService)
{
    public async Task RegisterEndpointAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var registerRequest = await context.Request.ReadJsonAsync<RegisterRequest>(ct);
        var normalizedRequest = registerRequest with { Email = registerRequest.Email.ToLowerInvariant().Trim() };
        var validationResult = await registerRequestValidator.ValidateAsync(normalizedRequest, ct);

        if (validationResult.IsValid)
        {
            var userId = await authService.RegisterUserAsync(normalizedRequest.Email, normalizedRequest.Password, ct);

            var (_, token) = await authService.CreateUserSessionAsync(userId, ct);

            context.Response.StatusCode = HttpStatusCode.Created;
            context.Response.SetContentLength(0);
            SetTokenCookie(context, token);
        }
        else
        {
            throw new ValidationException("Incorrect register request", validationResult.Errors);
        }
    }
    
    public async Task GetMeEndpointAsync(HttpContext context)
    {
        var user = context.Request.UserClaims;
        if (user == null) ExceptionsHelpers.ThrowUnauthorized();
        context.Response.WriteJson(user);
        context.Response.StatusCode = HttpStatusCode.OK;
    }

    public async Task LoginEndpointAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var (email, password) = await context.Request.ReadJsonAsync<LoginRequest>(ct);

        var user = await authService.LoginUserAsync(email, password, ct);
        var (_, token) = await authService.CreateUserSessionAsync(user.Id, ct);
        
        context.Response.SetContentLength(0);
        SetTokenCookie(context, token);
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