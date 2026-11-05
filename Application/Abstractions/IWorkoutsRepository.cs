using Domain.Models.Workouts;

namespace Application.Abstractions;

public interface IWorkoutsRepository
{
    Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<Workout> GetCurrentAsync(Guid userId, string title, CancellationToken ct);
    Task<IEnumerable<Workout>> GetAllAsync(Guid userId, CancellationToken ct);
    Task<bool> EndCurrentAsync(Guid userId, CancellationToken ct);
}