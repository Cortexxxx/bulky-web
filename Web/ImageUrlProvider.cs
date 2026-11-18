using Amazon.S3;
using Amazon.S3.Model;

namespace Web;

public class ImageUrlProvider(IAmazonS3 s3Client)
{
    private const string BucketName = "bulky-images";
    private const int ImageUrlLifetimeMinutes = 5;

    public string? GetPreSignedImageUrl(string? imageKey)
    {
        return imageKey is null ? null : s3Client.GetPreSignedURL(CreateRequest(imageKey, HttpVerb.GET));
    }

    public Task<string> GetPreSignedPutImageUrlAsync(string imageKey) =>
        s3Client.GetPreSignedURLAsync(CreateRequest(imageKey, HttpVerb.PUT));

    private static GetPreSignedUrlRequest CreateRequest(string imageKey, HttpVerb verb) => new()
    {
        Verb = verb,
        Protocol = Protocol.HTTP,
        BucketName = BucketName,
        Key = imageKey,
        Expires = DateTime.UtcNow.AddMinutes(ImageUrlLifetimeMinutes)
    };
}
