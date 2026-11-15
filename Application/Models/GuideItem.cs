namespace Application.Models;

public record GuideItem
{
    public GuideCategory Category { get; init; }
    public Guid SourceId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? ImagePath { get; init; }
}
