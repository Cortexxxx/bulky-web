using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class MusclesService(IMuscleGroupsRepository muscleGroupsRepository)
{
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync(CancellationToken ct)
    {
        return await muscleGroupsRepository.GetAllAsync(ct);
    }

    public async Task<MuscleGroup?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await muscleGroupsRepository.GetByIdAsync(id, ct);
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