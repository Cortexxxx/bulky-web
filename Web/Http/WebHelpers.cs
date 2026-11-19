using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using Application.Models;
using Domain.Models.Auth;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Web.Exceptions;
using Web.Routing;
using Web.Validators;

namespace Web.Http;

public static class WebHelpers
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public static string[] GetPathParts(string path) => path
            .Replace('\\', '/')
            .Split('/')
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

    public static JsonSerializerOptions? GetJsonSerializerOptions() => Options;

    public static Guid GetIdOrThrowNotFound(HttpContext context)
    {
        if (!Router.TryGetGuidParameter(context.Request.RouteParameters, "id", out var id))
        {
            ExceptionsHelpers.ThrowNotFound("Invalid Id parameter");
        }

        return id.GetValueOrDefault();
    }

    public static RouteKind GetRouteKind(HttpRequest request)
    {
        if (request.Route == null)
        {
            return request.Url.LocalPath.StartsWith("/api/", StringComparison.Ordinal) 
                ? RouteKind.Api 
                : RouteKind.Page;
        }

        return request.Route.RouteKind;
    }
    
    public static Task<TRequest> ProceedRequestAsync<TRequest>(HttpContext context,
        IValidator<TRequest> validator, CancellationToken ct)
        => ProceedRequestAsync(context, validator, request => request, ct);

    public static async Task<TRequest> ProceedRequestAsync<TRequest>(HttpContext context,
        IValidator<TRequest> validator, Func<TRequest, TRequest> normalizeFunc, CancellationToken ct)
    {
        var exerciseRequest = await context.Request.ReadJsonAsync<TRequest>(ct);
        var normalizedRequest = normalizeFunc(exerciseRequest);

        await ValidateRequestAsync(validator, normalizedRequest, ct);

        return normalizedRequest;
    }
    
    public static async Task ValidateRequestAsync<TRequest>(
        IValidator<TRequest> validator, TRequest request, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
    
    public static HttpStatusCode GetStatusCode(this DomainException e) => e switch
    {
        NotFoundException     => HttpStatusCode.NotFound,
        ConflictException     => HttpStatusCode.Conflict,
        UnauthorizedException => HttpStatusCode.Unauthorized,
        BadRequestException   => HttpStatusCode.BadRequest,
        _ => HttpStatusCode.BadRequest
    };

    public static string GetEnviromentVariableOrThrow(string name)
    {
        return Environment.GetEnvironmentVariable(name)
               ?? throw new Exception($"Environment variable '{name}' is not set"); 
    }

    public static ValidationOptions GetValidationOptions(this IConfiguration configuration)
    {
        var options = configuration.GetRequiredSection("Validation").Get<ValidationOptions>()
            ?? throw new InvalidOperationException("Validation settings are required.");
        options.Validate();
        return options;
    }
    
    public static TimeSpan GetCacheTtl(this IConfiguration configuration)
    {
        var cacheTtlSeconds = configuration.GetValue<int>("Redis:DefaultCacheTtlSeconds");
        if (cacheTtlSeconds <= 0)
            throw new InvalidOperationException("Redis:DefaultCacheTtlSeconds must be greater than zero.");
        return TimeSpan.FromSeconds(cacheTtlSeconds);
    }
    
    public static int GetSessionLifetime(this IConfiguration configuration)
    {
        var sessionLifetimeMinutes = configuration.GetValue<int>("Sessions:SessionLifetimeMinutes");
        if (sessionLifetimeMinutes <= 0)
            throw new InvalidOperationException("Sessions:SessionLifetimeMinutes must be greater than zero.");
        return sessionLifetimeMinutes;
    }
    
    public static ExerciseHintOptions GetExerciseHintOptions(this IConfiguration configuration)
    {
        var exerciseHint = configuration.GetSection("ExerciseHint").Get<ExerciseHintOptions>() 
                                     ?? throw new InvalidOperationException("Exercise hint settings are required.");
        exerciseHint.Validate();
        return exerciseHint;
    }
}