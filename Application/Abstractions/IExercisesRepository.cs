using Domain.Models.Workouts;

namespace Application.Abstractions;

public interface IExercisesRepository
{
    Task<IEnumerable<Exercise>> GetAllAsync(bool showArchived, CancellationToken ct);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken ct);
    Task CreateAsync(Exercise exercise, CancellationToken ct);
    Task<bool> UpdateAsync(Exercise exercise, CancellationToken ct);
    Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct);
}