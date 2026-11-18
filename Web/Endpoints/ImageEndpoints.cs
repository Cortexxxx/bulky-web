using System.Net;
using Web.DTOs;
using Web.Http;
using WebException = Web.Exceptions.WebException;

namespace Web.Endpoints;

public class ImageEndpoints(ImageUrlProvider imageUrlProvider)
{
    public async Task CreatePutImageUrlAsync(HttpContext context)
    {
        var request = await context.Request.ReadJsonAsync<FileRequest>(context.CancellationToken);
        var keyPrefix = GetKeyPrefix(request.EntityType);
        var extension = GetImageExtension(request.FileType);
        var key = $"{keyPrefix}/{Guid.NewGuid()}.{extension}";
        var url = await imageUrlProvider.GetPreSignedPutImageUrlAsync(key);
        context.Response.WriteJson(new ImageUploadUrlResponse(key, url));
    }

    private static string GetKeyPrefix(string? entityType) => entityType switch
    {
        "exercise" => "exercises",
        "muscleGroup" => "muscle-groups",
        _ => throw new WebException("Unsupported image entity type", HttpStatusCode.BadRequest,
            "unsupported_image_entity_type")
    };

    private static string GetImageExtension(string? fileType) => fileType?.Trim().ToLowerInvariant() switch
    {
        "image/jpeg" => "jpg",
        "image/png" => "png",
        "image/webp" => "webp",
        "image/gif" => "gif",
        "image/avif" => "avif",
        _ => throw new WebException("Unsupported image type", HttpStatusCode.BadRequest, "unsupported_image_type")
    };
}
