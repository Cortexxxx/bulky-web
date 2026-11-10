namespace Web.DTOs.Workout;

public record WorkoutSetRequest
{
    public int Repeats { get; init; }
    public decimal Weight { get; init; }
}