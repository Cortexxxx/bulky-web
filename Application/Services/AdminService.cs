using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class AdminService(IMuscleGroupsRepository muscleGroupsRepository)
{
    public async Task<IEnumerable<MuscleGroup>> GetAllMuscleGroups(CancellationToken ct)
    {
        return await muscleGroupsRepository.GetAllAsync(ct);
    }

    public async Task<Guid> CreateMuscleGroup(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.CreateAsync(muscleGroup, ct);
    }

    public async Task<bool> UpdateMuscleGroup(MuscleGroup muscleGroup, CancellationToken ct)
    {
        return await muscleGroupsRepository.UpdateAsync(muscleGroup, ct);
    }
    
    public async Task<bool> SetArchivedMuscleGroup(Guid id, bool isArchived, CancellationToken ct)
    {
        return await muscleGroupsRepository.SetArchivedAsync(id, isArchived, ct);
    }
}