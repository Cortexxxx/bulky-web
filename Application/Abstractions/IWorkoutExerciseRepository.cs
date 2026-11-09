namespace Application.Abstractions;

public interface IWorkoutExerciseRepository
{
    Task<Guid> CreateWorkoutExerciseAsync(Guid userId, Guid exerciseId, CancellationToken ct);
    Task<bool> DeleteWorkoutExerciseAsync(Guid userId, Guid id);

    Task<Guid> CreateSetAsync(Guid userId, Guid workoutExerciseId, int repeats, decimal weight, CancellationToken ct);
    Task<Guid> UpdateSetAsync(Guid userId, Guid id, int repeats, decimal weight, CancellationToken ct);
    Task<Guid> DeleteSetAsync(Guid userId, Guid id, CancellationToken ct);
}