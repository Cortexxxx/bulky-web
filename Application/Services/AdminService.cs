using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class AdminService(IMuscleGroupsRepository muscleGroupsRepository)
{
    public async Task<IEnumerable<MuscleGroup>> GetAllMuscleGroupsAsync(CancellationToken ct)
    {
        return await muscleGroupsRepository.GetAllAsync(ct);
    }

    public async Task<MuscleGroup?> GetMuscleGroupByIdAsync(Guid id, CancellationToken ct)
    {
        return await muscleGroupsRepository.GetByIdAsync(id, ct);
    }

    public async Task<Guid> CreateMuscleGroupAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.CreateAsync(muscleGroup, ct);
    }

    public async Task<bool> UpdateMuscleGroupAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.UpdateAsync(muscleGroup, ct);
    }
    
    public async Task<bool> SetArchivedMuscleGroupAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        return await muscleGroupsRepository.SetArchivedAsync(id, isArchived, ct);
    }
}