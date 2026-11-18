using Application.Abstractions;
using Application.Models;
using StackExchange.Redis;

namespace Persistence.Redis;

public class CachedAnalyticsQueries(IAnalyticsQueries analyticsQueries, IDatabaseAsync source) : IAnalyticsQueries
{
    public async Task<List<WorkoutWeekStats>> GetWorkoutWeeksStats(Guid userId, CancellationToken ct)
    {
        var version = await source.GetVersion(RedisKeys.AnalyticsVersion(userId));
        
        var key = RedisKeys.WorkoutOverview(version, userId);

        var value = await source.GetCachedAsync<List<WorkoutWeekStats>>(key);

        if (value is not null) return value;
        
        value = await analyticsQueries.GetWorkoutWeeksStats(userId, ct);
        await source.SetCacheAsync(key, value);

        return value;
    }

    public async Task<List<MuscleWeekStats>> GetMuscleWeekStats(Guid userId, Guid muscleId, CancellationToken ct)
    {
        var version = await source.GetVersion(RedisKeys.AnalyticsVersion(userId));
        var key = RedisKeys.MuscleGroupOverview(version, userId, muscleId);
        var value = await source.GetCachedAsync<List<MuscleWeekStats>>(key);

        if (value is not null) return value;
        
        value = await analyticsQueries.GetMuscleWeekStats(userId, muscleId, ct);
        await source.SetCacheAsync(key, value);

        return value;
    }

    public async Task<List<ExerciseWeekSet>> GetExerciseWeekSets(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        var version = await source.GetVersion(RedisKeys.AnalyticsVersion(userId));

        var key = RedisKeys.ExerciseOverview(version, userId, exerciseId);
        var value = await source.GetCachedAsync<List<ExerciseWeekSet>>(key);

        if (value is not null) return value;
        
        value = await analyticsQueries.GetExerciseWeekSets(userId, exerciseId, ct);
        await source.SetCacheAsync(key, value);

        return value;
    }
}