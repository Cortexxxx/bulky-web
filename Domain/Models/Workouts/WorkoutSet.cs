namespace Domain.Models.Workouts;

public record struct WorkoutSet
{
    public Guid Id { get; init; }
    public Guid WorkoutId { get; init; }
    public Guid ExerciseId { get; init; }
    public short SetNumber { get; init; }
    public short ExerciseNumber { get; init; }
    public short Repeats { get; init; }
    public decimal Weight { get; init; }
}
