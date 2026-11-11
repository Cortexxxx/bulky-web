namespace Web.DTOs.Analytics;

public record OverviewResponse
{
    public int LastWeekWorkouts { get; init; }
    public int LastWeekWorkoutsIncrease { get; init; }

    public int LastWeekSets { get; init; }
    public int LastWeekSetsIncrease { get; init; }

    public int LastMonthWorkouts { get; init; }
    public int LastMonthWorkoutsIncrease { get; init; }

    public int LastMonthSets { get; init; }
    public int LastMonthSetsIncrease { get; init; }

    public int AverageDurationMinutes { get; init; }

    // Ряды для графиков: индекс 0 = последние 7 дней, дальше в прошлое
    public List<int> WeeksVolumes { get; init; } = [];
    public List<int> WeeksSets { get; init; } = [];
}
