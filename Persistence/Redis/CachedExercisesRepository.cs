using System.Text.Json;
using Application.Abstractions;
using Domain.Models.Workouts;
using StackExchange.Redis;

namespace Persistence.Redis;

public class CachedExercisesRepository(IDatabaseAsync cacheDataSource, IExercisesRepository exerciseRepository) : IExercisesRepository
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        var key = RedisKeys.ExercisesList(includeArchived);
        var data = await RedisHelper.ProceedRedisRequest(() 
            => cacheDataSource.StringGetAsync(key), RedisValue.Null);
        var exercises = RedisHelper.GetCacheOrNull<List<Exercise>>(data);

        if (exercises is null)
        {
            exercises = (await exerciseRepository.GetAllAsync(includeArchived, ct)).ToList();
            
            await RedisHelper.SetCacheAsync(key, exercises, cacheDataSource);
        }
        
        return exercises;
    }

    public async Task<Exercise?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        var key = RedisKeys.ExerciseById(id);
        var data = await RedisHelper.ProceedRedisRequest(() => 
            cacheDataSource.StringGetAsync(key), RedisValue.Null);
        var exercise = RedisHelper.GetCacheOrNull<Exercise>(data);

        if (exercise is null)
        {
            exercise = await exerciseRepository.GetByIdAsync(includeArchived: true, id, ct);
            
            if (exercise is null)
            {
                return null;
            }

            await RedisHelper.SetCacheAsync(key, exercise, cacheDataSource);
        }

        return includeArchived || !exercise.IsArchived ? exercise : null;
    }

    public async Task CreateAsync(Exercise exercise, CancellationToken ct)
    {
        await exerciseRepository.CreateAsync(exercise, ct);
        await InvalidateExercisesListAsync();
    }

    public async Task<bool> UpdateAsync(Exercise exercise, CancellationToken ct)
    {
        var found = await exerciseRepository.UpdateAsync(exercise, ct);
        if (found) await InvalidateExerciseAsync(exercise.Id);
        return found;
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        var found = await exerciseRepository.SetArchivedAsync(id, isArchived, ct);
        if (found) await InvalidateExerciseAsync(id);
        return found;
    }
    
    private async Task InvalidateExerciseAsync(Guid id)
    {
        await RedisHelper.InvalidateCacheAsync(RedisKeys.ExerciseById(id), cacheDataSource);
        await InvalidateExercisesListAsync();
    }

    private async Task InvalidateExercisesListAsync()
    {
        await RedisHelper.InvalidateCacheAsync(RedisKeys.ExercisesList(true), cacheDataSource);
        await RedisHelper.InvalidateCacheAsync(RedisKeys.ExercisesList(false), cacheDataSource);
    }
}
