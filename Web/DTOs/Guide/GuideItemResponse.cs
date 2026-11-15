using Application.Models;

namespace Web.DTOs.Guide;

public record GuideItemResponse
{
    public GuideCategory Category { get; init; }
    public Guid SourceId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? ImagePath { get; init; }
}
