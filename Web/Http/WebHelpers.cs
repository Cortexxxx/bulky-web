using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application;
using Domain.Models.Auth;
using FluentValidation;
using Web.Exceptions;
using Web.Routing;

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

    public static async Task<TRequest> ProceedRequestAsync<TRequest, TValidator>(HttpContext context, 
        TValidator validator, Func<TRequest, TRequest> normalizeFunc, CancellationToken ct) 
        where TValidator : AbstractValidator<TRequest>
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
}