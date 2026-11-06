using Application.Abstractions;
using Domain.Models.Workouts;

namespace Application.Services;

public class ExercisesService(IExercisesRepository repository)
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        return await repository.GetAllAsync(includeArchived, ct);
    }

    public async Task<Exercise> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        return await repository.GetByIdAsync(includeArchived, id, ct) ?? throw new NotFoundException("unknown_exercise");
    }

    public async Task<Guid> CreateAsync(string title, string description, string? imagePath, 
        List<ExerciseMuscleGroup> exerciseMuscles, CancellationToken ct)
    {
        var exerciseId = Guid.NewGuid();
        await repository.CreateAsync(CreateExercise(exerciseId, title, description, imagePath, exerciseMuscles), ct);

        return exerciseId;
    }

    public async Task UpdateAsync(Guid id, string title, string description, string? imagePath, 
        List<ExerciseMuscleGroup> exerciseMuscles, CancellationToken ct)
    {
        if (!await repository.UpdateAsync(CreateExercise(id, title, description, imagePath, exerciseMuscles), ct))
        {
            throw new NotFoundException("unknown_exercise");
        }
    }
    public async Task SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        if (!await repository.SetArchivedAsync(id, isArchived, ct))
        {
            throw new NotFoundException("unknown_exercise");
        }
    }
    
    private static Exercise CreateExercise(Guid id, string title, string description, string? imagePath, 
        List<ExerciseMuscleGroup> exerciseMuscles)
    {
        return new Exercise
        {
            Id = id,
            Title = title,
            Description = description,
            ImagePath = imagePath,
            MuscleGroups = exerciseMuscles
        };
    }
}