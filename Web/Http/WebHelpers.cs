using System.Text.Json;
using System.Text.Json.Serialization;
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

    public static async Task ValidateRequestAsync<T>(
        IValidator<T> validator, T request, string errorMessage, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(errorMessage, validationResult.Errors);
        }
    }

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
}