using Application.Abstractions;
using Domain.Models.Workouts;
using StackExchange.Redis;

namespace Persistence.Redis;

public class CachedExercisesRepository(IDatabaseAsync source, IExercisesRepository exerciseRepository, TimeSpan cacheTtl)
    : IExercisesRepository
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        var key = RedisKeys.ExercisesList(includeArchived);
        var exercises = await source.GetCachedAsync<List<Exercise>>(key);

        if (exercises is not null) return exercises;
        
        exercises = (await exerciseRepository.GetAllAsync(includeArchived, ct)).ToList();
        await source.SetCacheAsync(key, exercises, cacheTtl);

        return exercises;
    }

    public async Task<Exercise?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        var key = RedisKeys.ExerciseById(id);
        var exercise = await source.GetCachedAsync<Exercise>(key);

        if (exercise is null)
        {
            exercise = await exerciseRepository.GetByIdAsync(includeArchived: true, id, ct);
            
            if (exercise is null)
            {
                return null;
            }

            await source.SetCacheAsync(key, exercise, cacheTtl);
        }

        return includeArchived || !exercise.IsArchived ? exercise : null;
    }

    public async Task CreateAsync(Exercise exercise, CancellationToken ct)
    {
        await exerciseRepository.CreateAsync(exercise, ct);
        await source.InvalidateExercisesListAsync();
    }

    public async Task<bool> UpdateAsync(Exercise exercise, CancellationToken ct)
    {
        var found = await exerciseRepository.UpdateAsync(exercise, ct);
        if (found) await source.InvalidateExerciseAsync(exercise.Id);
        return found;
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        var found = await exerciseRepository.SetArchivedAsync(id, isArchived, ct);
        if (found) await source.InvalidateExerciseAsync(id);
        return found;
    }
}
