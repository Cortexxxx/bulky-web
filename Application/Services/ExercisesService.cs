using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class ExercisesService(IExercisesRepository repository)
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken ct)
    {
        var exercises = await repository.GetAllAsync(true, ct);
        return exercises;
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await repository.GetByIdAsync(id, ct);
    }

    public async Task<Guid> CreateAsync(string title, string description, string? imagePath, 
        IEnumerable<ExerciseMuscle> exerciseMuscles, CancellationToken ct)
    {
        var exerciseId = Guid.NewGuid();
        await repository.CreateAsync(CreateExercise(exerciseId, title, description, imagePath, exerciseMuscles), ct);

        return exerciseId;
    }

    public async Task<bool> UpdateAsync(Guid id, string title, string description, string? imagePath, 
        IEnumerable<ExerciseMuscle> exerciseMuscles, CancellationToken ct)
    {
        return await repository.UpdateAsync(CreateExercise(id, title, description, imagePath, exerciseMuscles), ct);
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        return await repository.SetArchivedAsync(id, isArchived, ct);
    }
    
    private static Exercise CreateExercise(Guid id, string title, string description, string? imagePath, IEnumerable<ExerciseMuscle> exerciseMuscles)
    {
        return new Exercise
        {
            Id = id,
            Title = title,
            Description = description,
            ImagePath = imagePath,
            MuscleGroups = exerciseMuscles.Select(m => new ExerciseMuscleGroup
            {
                ExerciseId = id,
                MuscleGroupId = m.MuscleGroupId,
                Role = m.MuscleGroupRole
            }).ToList()
        };
    }
}