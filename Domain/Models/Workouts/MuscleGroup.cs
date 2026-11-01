namespace Domain.Models.Workouts;

public record struct MuscleGroup
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string? ImagePath { get; init; }
    public bool IsArchived { get; init; }
}
