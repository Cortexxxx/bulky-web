namespace Web.DTOs.Workout;

public record WorkoutSetResponse
{
    public Guid Id { get; init; }
    public short Repeats { get; init; }
    public decimal Weight { get; init; }
}
