using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class WorkoutsService(IWorkoutsRepository workoutsRepository)
{
    public Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct)
    {
        return workoutsRepository.CreateAsync(userId, title, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        return workoutsRepository.DeleteAsync(id, ct);
    }

    public Task<Workout?> GetCurrentAsync(Guid userId, CancellationToken ct)
    {
        return workoutsRepository.GetCurrentAsync(userId, ct);
    }

    public Task<IEnumerable<Workout>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        return workoutsRepository.GetAllAsync(userId, ct);
    }

    public Task<bool> EndCurrentAsync(Guid userId, CancellationToken ct)
    {
        return workoutsRepository.EndCurrentAsync(userId, ct);
    }
}
