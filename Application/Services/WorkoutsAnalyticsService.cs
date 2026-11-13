using Application.Abstractions;
using Application.Models;

namespace Application.Services;

public class WorkoutsAnalyticsService(IAnalyticsQueries analyticsQueries)
{
    public async Task<WorkoutsOverview> GetWorkoutsOverview(Guid userId, CancellationToken ct)
    {
        var weekStats = await analyticsQueries.GetWorkoutWeeksStats(userId, ct);
        var totalWorkouts = weekStats.Sum(w => w.WorkoutsCount);
        var overview = new WorkoutsOverview
        {
            LastWeekWorkouts = weekStats.First().WorkoutsCount,
            LastWeekWorkoutsIncrease = weekStats.First().WorkoutsCount - weekStats.Skip(1).First().WorkoutsCount,
            LastWeekSets = weekStats.First().TotalSets,
            LastWeekSetsIncrease = weekStats.First().TotalSets - weekStats.Skip(1).First().TotalSets,
            LastMonthWorkouts = weekStats.Take(4).Sum(s => s.WorkoutsCount),
            LastMonthWorkoutsIncrease = weekStats.Take(4).Sum(s => s.WorkoutsCount) - weekStats.Skip(4).Take(4).Sum(s => s.WorkoutsCount),
            LastMonthSets = weekStats.Take(4).Sum(s => s.TotalSets),
            LastMonthSetsIncrease = weekStats.Take(4).Sum(s => s.TotalSets) - weekStats.Skip(4).Take(4).Sum(s => s.TotalSets),
            AverageDuration = totalWorkouts == 0 ? 0 : (int)weekStats.Sum(w => w.TotalTime.TotalMinutes) / totalWorkouts,
            WeeksVolumes = weekStats.Select(w => (int)w.TotalVolume).ToList(),
            WeeksSets = weekStats.Select(w => w.TotalSets).ToList()
        };

        return overview;
    }

    public async Task<MuscleGroupOverview> GetMuscleGroupOverview(Guid userId, Guid muscleId, CancellationToken ct)
    {
        var weekStats = await analyticsQueries.GetMuscleWeekStats(userId, muscleId, ct);

        var lastWeek = weekStats.First();
        var previousWeek = weekStats.Skip(1).First();
        var lastMonthVolume = weekStats.Take(4).Sum(s => s.TotalVolume);
        var previousMonthVolume = weekStats.Skip(4).Take(4).Sum(s => s.TotalVolume);

        var overview = new MuscleGroupOverview
        {
            LastWeekVolume = lastWeek.TotalVolume,
            LastWeekVolumeIncrease = previousWeek.TotalVolume == 0
                ? 0
                : (lastWeek.TotalVolume - previousWeek.TotalVolume) * 100 / previousWeek.TotalVolume,
            LastMonthVolume = lastMonthVolume,
            LastMonthVolumeIncreasePercents = previousMonthVolume == 0
                ? 0
                : (lastMonthVolume - previousMonthVolume) * 100 / previousMonthVolume,
            LastWeekSets = lastWeek.TotalSets,
            LastWeekSetsIncrease = lastWeek.TotalSets - previousWeek.TotalSets,
            AverageWeekSets = (int)Math.Round(weekStats.Average(s => s.TotalSets)),
            AverageWeekVolume = (int)Math.Round(weekStats.Average(s => s.TotalVolume)),
            WeeksVolumes = weekStats.Select(s => s.TotalVolume).ToList(),
            WeeksSets = weekStats.Select(s => s.TotalSets).ToList()
        };

        return overview;
    }

    public async Task<ExercisesOverview> GetExerciseOverview(Guid userId, Guid muscleId, CancellationToken ct)
    {
        var exerciseWeekSets = await analyticsQueries.GetExerciseWeekSets(userId, muscleId, ct);
        var weekGroups = exerciseWeekSets.GroupBy(w => w.WeekNumber).ToDictionary(sets => sets.Key, sets => sets.ToList());
        
        for (var i = 0; i < 12; i++)
        {
            if (!weekGroups.ContainsKey(i))
            {
                weekGroups[i] = [];
            }
        }

        var maxWeekOrms = weekGroups.Select(pair => pair.Value.Select(CalculateOrm).Max()).ToList();
        var lastWeekOrm = maxWeekOrms[0];
        var lastMonthOrm = maxWeekOrms.Take(4).Max();
        
        var overview = new ExercisesOverview
        {
            LastWeekOrm = lastWeekOrm,
            LastWeekOrmIncrease = lastWeekOrm - maxWeekOrms[1],
            LastMonthOrm = lastMonthOrm,
            LastMonthOrmIncrease = lastMonthOrm - maxWeekOrms.Skip(4).Take(4).Max(),
            MaxOrm = maxWeekOrms.Max(),
            MaxVolume = (int)exerciseWeekSets.Select(w => w.Weight * w.Repeats).Max(),
            MaxWeight = (int)exerciseWeekSets.Select(w => w.Weight).Max(),
            Orms = maxWeekOrms,
            Volumes = weekGroups.Select(pair => pair.Value.Sum(w => (int)(w.Repeats * w.Weight))).ToList()
        };

        return overview;
    }

    private static decimal CalculateOrm(ExerciseWeekSet w)
    {
        return w.Weight * (1 + w.Repeats / 30m);
    }
}