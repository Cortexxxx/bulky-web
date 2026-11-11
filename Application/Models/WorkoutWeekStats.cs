namespace Application.Models;

public record WorkoutWeekStats
{
    public int WeekNumber { get; init; }
    public int WorkoutsCount { get; init; }
    public TimeSpan TotalTime { get; init; }
    public int TotalSets { get; init; }
    public decimal TotalVolume { get; init; }
}