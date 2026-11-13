namespace Application.Models;

public record ExerciseWeekSet
{
    public int WeekNumber { get; init; }
    public int Repeats { get; init; }
    public decimal Weight { get; init; }
}