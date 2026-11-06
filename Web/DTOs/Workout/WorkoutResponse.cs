namespace Web.DTOs.Workout;

public class WorkoutResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
}