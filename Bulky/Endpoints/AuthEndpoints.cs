using System.Net;
using Bulky.Exceptions;
using Bulky.Http;
using Bulky.Models;
using Bulky.Services;
using Bulky.Validators;
using ValidationException = FluentValidation.ValidationException;

namespace Bulky.Endpoints;

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
            
            context.Response.SetCookie("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(5)
            });
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
}