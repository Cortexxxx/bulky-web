namespace Domain.Models.Workouts;

public record WorkoutSet
{
    public required Guid Id { get; init; }
    public required short Repeats { get; init; }
    public required decimal Weight { get; init; }
}
