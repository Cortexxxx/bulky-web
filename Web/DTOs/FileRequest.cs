namespace Web.DTOs;

public record FileRequest()
{
    public string? EntityType { get; init; }
    public required string FileType { get; init; }
    public long ContentLength { get; init; }
};
