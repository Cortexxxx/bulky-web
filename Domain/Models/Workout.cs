namespace Domain.Models;

public record struct Workout
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? EndedAt { get; init; }
}
