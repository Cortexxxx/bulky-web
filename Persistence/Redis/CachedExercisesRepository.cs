using System.Text.Json;
using Application.Abstractions;
using Domain.Models.Workouts;
using StackExchange.Redis;

namespace Persistence.Redis;

public class CachedExercisesRepository(IDatabaseAsync cacheDataSource, IExercisesRepository exerciseRepository) : IExercisesRepository
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        var key = $"bulky:exercises:v1:{(includeArchived ? "all" : "active")}";
        
        var data = await RedisHelper.ProceedRedisRequest(() => cacheDataSource.StringGetAsync(key), RedisValue.Null);

        var exercises = data.HasValue
            ? JsonSerializer.Deserialize<List<Exercise>>(data.ToString(), JsonSerializerOptions.Web)
            : null;

        if (exercises is null)
        {
            exercises = (await exerciseRepository.GetAllAsync(includeArchived, ct)).ToList();

            await RedisHelper.ProceedRedisRequest(() => cacheDataSource.StringSetAsync(key,
                JsonSerializer.Serialize(exercises, JsonSerializerOptions.Web),
                new Expiration(TimeSpan.FromSeconds(60))), false);
        }
        
        return exercises;
    }

    public async Task<Exercise?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        var key = $"bulky:exercises:v1:{id}";
        var data = await RedisHelper.ProceedRedisRequest(() => cacheDataSource.StringGetAsync(key), RedisValue.Null);

        var exercise = data.HasValue
            ? JsonSerializer.Deserialize<Exercise>(data.ToString(), JsonSerializerOptions.Web)
            : null;

        if (exercise is null)
        {
            exercise = await exerciseRepository.GetByIdAsync(includeArchived: true, id, ct);
            if (exercise is null)
            {
                return null;
            }

            await RedisHelper.ProceedRedisRequest(() => cacheDataSource.StringSetAsync(key,
                JsonSerializer.Serialize(exercise, JsonSerializerOptions.Web),
                new Expiration(TimeSpan.FromSeconds(60))), false);
        }

        return includeArchived || !exercise.IsArchived ? exercise : null;
    }

    public Task CreateAsync(Exercise exercise, CancellationToken ct)
    {
        return exerciseRepository.CreateAsync(exercise, ct);
    }

    public Task<bool> UpdateAsync(Exercise exercise, CancellationToken ct)
    {
        return exerciseRepository.UpdateAsync(exercise, ct);
    }

    public Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        return exerciseRepository.SetArchivedAsync(id, isArchived, ct);
    }
}
