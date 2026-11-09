namespace Domain.Models.Workouts;

public record Exercise
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? ImagePath { get; init; }
    public bool IsArchived { get; init; }
    public List<ExerciseMuscleGroup> MuscleGroups { get; init; } = [];
}
