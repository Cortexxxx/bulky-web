namespace Domain.Models.Workouts;

public record Exercise
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string? ImagePath { get; init; }
    public bool IsArchived { get; init; }
    public List<ExerciseMuscleGroup> MuscleGroups { get; init; } = [];
}
