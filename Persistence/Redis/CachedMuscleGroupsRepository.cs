using Application.Abstractions;
using Domain.Models.Workouts;
using StackExchange.Redis;

namespace Persistence.Redis;

public class CachedMuscleGroupsRepository(IDatabaseAsync source, IMuscleGroupsRepository muscleGroupsRepository, TimeSpan cacheTtl)
    : IMuscleGroupsRepository
{
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        var key = RedisKeys.MuscleGroupsList(includeArchived);
        var value = await source.GetCachedAsync<List<MuscleGroup>>(key);

        if (value is null)
        {
            value = (await muscleGroupsRepository.GetAllAsync(includeArchived, ct)).ToList();
            
            await source.SetCacheAsync(key, value, cacheTtl);
        }
        
        return value;
    }

    public async Task<MuscleGroup?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        var key = RedisKeys.MuscleGroupById(id);
        var value = await source.GetCachedAsync<MuscleGroup>(key);

        if (value is null)
        {
            value = await muscleGroupsRepository.GetByIdAsync(includeArchived: true, id, ct);
            
            if (value is null)
            {
                return null;
            }

            await source.SetCacheAsync(key, value, cacheTtl);
        }

        return includeArchived || !value.IsArchived ? value : null;
    }

    public async Task<Guid> CreateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        var guid = await muscleGroupsRepository.CreateAsync(muscleGroup, ct);
        await source.InvalidateMuscleGroupsListAsync();
        return guid;
    }

    public async Task<bool> UpdateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        var found = await muscleGroupsRepository.UpdateAsync(muscleGroup, ct);
        if (found) await source.InvalidateMuscleGroupAsync(muscleGroup.Id);
        return found;
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        var found = await muscleGroupsRepository.SetArchivedAsync(id, isArchived, ct);
        if (found) await source.InvalidateMuscleGroupAsync(id);
        return found;
    }
}
