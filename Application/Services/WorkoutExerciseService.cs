using Application.Abstractions;

namespace Application.Services;

public class WorkoutExerciseService(IWorkoutExerciseRepository workoutExerciseRepository)
{
    public async Task<Guid> CreateAsync(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        return await workoutExerciseRepository.CreateWorkoutExerciseAsync(userId, exerciseId, ct);
    }
}