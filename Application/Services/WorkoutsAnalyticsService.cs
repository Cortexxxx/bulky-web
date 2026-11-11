using Application.Abstractions;
using Application.Models;

namespace Application.Services;

public class WorkoutsAnalyticsService(IAnalyticsQueries analyticsQueries)
{
    public async Task<WorkoutsOverview> GetWorkoutsOverview(Guid userId, CancellationToken ct)
    {
        var weekStats = await analyticsQueries.GetWorkoutWeeksStats(userId, ct);

        var overview = new WorkoutsOverview
        {
            LastWeekWorkouts = weekStats.First().WorkoutsCount,
            LastWeekWorkoutsIncrease = weekStats.First().WorkoutsCount - weekStats.Skip(1).First().WorkoutsCount,
            LastWeekSets = weekStats.First().TotalSets,
            LastWeekSetsIncrease = weekStats.Skip(1).First().TotalSets - weekStats.First().TotalSets,
            LastMonthWorkouts = weekStats.Take(4).Sum(s => s.WorkoutsCount),
            LastMonthWorkoutsIncrease = weekStats.Take(4).Sum(s => s.WorkoutsCount) - weekStats.Skip(4).Take(4).Sum(s => s.WorkoutsCount),
            LastMonthSets = weekStats.Take(4).Sum(s => s.TotalSets),
            LastMonthSetsIncrease = weekStats.Take(4).Sum(s => s.TotalSets) - weekStats.Skip(4).Take(4).Sum(s => s.TotalSets),
            AverageDuration = (int)weekStats.Sum(w => w.TotalTime.TotalMinutes) / weekStats.Sum(w => w.WorkoutsCount),
            WeeksVolumes = weekStats.Select(w => (int)w.TotalVolume).ToList(),
            WeeksSets = weekStats.Select(w => w.TotalSets).ToList()
        };

        return overview;
    }
}