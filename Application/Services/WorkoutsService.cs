using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class WorkoutsService(IWorkoutsRepository workoutsRepository)
{
    public async Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct)
    {
        return await workoutsRepository.CreateAsync(userId, title, ct);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        await workoutsRepository.DeleteAsync(userId, id, ct);
    }

    public async Task<Workout?> GetCurrentAsync(Guid userId, CancellationToken ct)
    {
        return await workoutsRepository.GetCurrentAsync(userId, ct);
    }

    public async Task<Workout> GetByIdAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var workout = await workoutsRepository.GetByIdAsync(userId, id, ct);
        
        if (workout == null)
        {
            throw new NotFoundException("workout_not_found");
        }

        return workout;
    }
    
    public async Task<IEnumerable<Workout>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        return await workoutsRepository.GetAllAsync(userId, ct);
    }

    public async Task EndCurrentAsync(Guid userId, CancellationToken ct)
    {
        if (!await workoutsRepository.EndCurrentAsync(userId, ct))
        {
            throw new NotFoundException("no_current_workout");
        }
    }
}
