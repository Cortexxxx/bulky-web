using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class MusclesService(IMuscleGroupsRepository muscleGroupsRepository)
{
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        return await muscleGroupsRepository.GetAllAsync(includeArchived, ct);
    }

    public async Task<MuscleGroup?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        return await muscleGroupsRepository.GetByIdAsync(includeArchived, id, ct);
    }

    public async Task<Guid> CreateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.CreateAsync(muscleGroup, ct);
    }

    public async Task<bool> UpdateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.UpdateAsync(muscleGroup, ct);
    }
    
    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        return await muscleGroupsRepository.SetArchivedAsync(id, isArchived, ct);
    }
}