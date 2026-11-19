using Application.Abstractions;
using Application.Models;

namespace Application.Services;

public class WorkoutExerciseService(IWorkoutExerciseRepository workoutExerciseRepository, ExerciseHintOptions exerciseHintOptions)
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
            throw new NotFoundException("workout_exercise_not_found");
        }
    }

    public async Task<Guid> CreateSetAsync(Guid userId, Guid workoutExerciseId, CancellationToken ct)
    {
        return await workoutExerciseRepository.CreateSetAsync(userId, workoutExerciseId, ct);
    }

    public async Task DeleteSetAsync(Guid userId, Guid workoutExerciseId, CancellationToken ct)
    {
        if (!await workoutExerciseRepository.DeleteSetAsync(userId, workoutExerciseId, ct))
        {
            throw new NotFoundException("set_not_found");
        }
    }

    public async Task UpdateSetAsync(Guid userId, Guid setId, int repeats, decimal weight, CancellationToken ct)
    {
        if (!await workoutExerciseRepository.UpdateSetAsync(userId, setId, repeats, weight, ct))
        {
            throw new NotFoundException("set_not_found");
        }
    }

    public async Task<ExerciseHint> GetExerciseHint(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        var sets = 
            await workoutExerciseRepository.GetLastCompletedExerciseSetsAsync(userId, exerciseId, ct);
        
        if (sets.Count == 0)
        {
            throw new Exception();
        }

        return new ExerciseHint(sets, exerciseHintOptions);
    }
}