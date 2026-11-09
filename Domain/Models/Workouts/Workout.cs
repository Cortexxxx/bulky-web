namespace Domain.Models.Workouts;

public record Workout
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
    public List<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
