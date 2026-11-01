using Domain.Models.Workouts;

namespace Application.Abstractions;

public interface IMuscleGroupsRepository
{
    Task<IEnumerable<MuscleGroup>> GetAllAsync(CancellationToken ct);
    Task<Guid> CreateAsync(MuscleGroup muscleGroup, CancellationToken ct);
    Task<bool> UpdateAsync(MuscleGroup muscleGroup, CancellationToken ct);
    Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct);
}