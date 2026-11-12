namespace Web.DTOs.Analytics;

public record MuscleGroupOverviewResponse
{
    public int LastWeekVolume { get; init; }
    public int LastWeekVolumeIncrease { get; init; }

    public int LastMonthVolume { get; init; }
    public int LastMonthVolumeIncreasePercents { get; init; }

    public int LastWeekSets { get; init; }
    public int LastWeekSetsIncrease { get; init; }

    public int AverageWeekSets { get; init; }
    public int AverageWeekVolume { get; init; }

    // Ряды для графиков: индекс 0 = последние 7 дней, дальше в прошлое
    public List<int> WeeksVolumes { get; init; } = [];
    public List<int> WeeksSets { get; init; } = [];
}
