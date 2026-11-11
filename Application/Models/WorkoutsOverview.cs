namespace Application.Models;

public record WorkoutsOverview
{
    public int LastWeekWorkouts { get; init; }
    public int LastWeekWorkoutsIncrease { get; init; }
    
    public int LastWeekSets { get; init; }
    public int LastWeekSetsIncrease { get; init; }
    
    public int LastMonthWorkouts { get; init; }
    public int LastMonthWorkoutsIncrease { get; init; }
    
    public int LastMonthSets { get; init; }
    public int LastMonthSetsIncrease { get; init; }

    public int AverageDuration { get; init; }
    public List<int> WeeksVolumes { get; init; } = [];
    public List<int> WeeksSets { get; init; } = [];
}