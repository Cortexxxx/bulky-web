using Application.Abstractions;

namespace Application.Services;

public class WorkoutExerciseService(IWorkoutExerciseRepository workoutExerciseRepository)
{
    public async Task<Guid> CreateExerciseAsync(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        return await workoutExerciseRepository.CreateWorkoutExerciseAsync(userId, exerciseId, ct);
    }
    
    public async Task DeleteExerciseAsync(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        var found = await workoutExerciseRepository.DeleteWorkoutExerciseAsync(userId, exerciseId, ct);

        if (!found)
        {
            throw new NotFoundException("unknown_workout_exercise");
        }
    }

    public async Task<Guid> CreateSetAsync(Guid userId, Guid workoutExerciseId, CancellationToken ct)
    {
        return await workoutExerciseRepository.CreateSetAsync(userId, workoutExerciseId, ct);
    }
    
}