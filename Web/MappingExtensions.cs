using Domain.Models.Auth;
using Domain.Models.Workouts;
using Web.DTOs.Auth;
using Web.DTOs.Exercise;
using Web.DTOs.MuscleGroup;
using Web.DTOs.Workout;

namespace Web;

public static class MappingExtensions
{
    public static MuscleGroupResponse ToResponse(this MuscleGroup muscleGroup) => new()
    {
        Id = muscleGroup.Id,
        Title = muscleGroup.Title,
        Description = muscleGroup.Description,
        ImagePath = muscleGroup.ImagePath,
        IsArchived = muscleGroup.IsArchived
    };

    public static ExerciseMuscleGroup ToDomain(this ExerciseMuscleDto exerciseMuscleDto) => new()
    {
        MuscleGroupId = exerciseMuscleDto.MuscleGroupId,
        Role = exerciseMuscleDto.Role
    };

    public static ExerciseMuscleDto ToResponse(this ExerciseMuscleGroup exerciseMuscleGroup) => 
        new(exerciseMuscleGroup.MuscleGroupId, exerciseMuscleGroup.Role);

    public static ExerciseResponse ToResponse(this Exercise exercise) => new() 
    { 
        Id = exercise.Id,
        Title = exercise.Title,
        Description = exercise.Description,
        ImagePath = exercise.ImagePath,
        IsArchived = exercise.IsArchived,
        MuscleGroups = exercise.MuscleGroups.Select(m => m.ToResponse()).ToList()
    };

    public static SessionResponse ToResponse(this Session session) => 
        new(session.Id, session.CreatedAt, session.ExpiresAt);

    public static WorkoutSetResponse ToResponse(this WorkoutSet set) => new()
    {
        Id = set.Id,
        Repeats = set.Repeats,
        Weight = set.Weight
    };

    public static WorkoutExerciseResponse ToResponse(this WorkoutExercise workoutExercise) => new()
    {
        Id = workoutExercise.Id,
        ExerciseId = workoutExercise.ExerciseId,
        ExerciseTitle = workoutExercise.ExerciseTitle,
        Sets = workoutExercise.Sets.Select(s => s.ToResponse()).ToList()
    };

    public static WorkoutResponse ToResponse(this Workout workout) => new()
    {
        Id = workout.Id,
        Title = workout.Title,
        StartedAt = workout.StartedAt,
        EndedAt = workout.EndedAt,
        Exercises = workout.WorkoutExercises.Select(e => e.ToResponse()).ToList()
    };
}