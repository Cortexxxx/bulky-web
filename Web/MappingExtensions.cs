using Domain.Models.Auth;
using Domain.Models.Workouts;
using Web.DTOs;
using Web.DTOs.Auth;

namespace Web;

public static class MappingExtensions
{
    public static MuscleGroupResponse ToResponse(this MuscleGroup muscleGroup)
    {
        return new MuscleGroupResponse
        {
            Id = muscleGroup.Id,
            Title = muscleGroup.Title,
            Description = muscleGroup.Description,
            ImagePath = muscleGroup.ImagePath,
            IsArchived = muscleGroup.IsArchived
        };
    }

    public static ExerciseMuscleGroup ToDomain(this ExerciseMuscleDto exerciseMuscleDto)
    {
        return new ExerciseMuscleGroup
        {
            MuscleGroupId = exerciseMuscleDto.MuscleGroupId,
            Role = exerciseMuscleDto.Role
        };
    }
    
    public static ExerciseMuscleDto ToResponse(this ExerciseMuscleGroup exerciseMuscleGroup)
    {
        return new ExerciseMuscleDto(exerciseMuscleGroup.MuscleGroupId, exerciseMuscleGroup.Role);
    }
    
    public static ExerciseResponse ToResponse(this Exercise exercise)
    {
        return new ExerciseResponse
        {
            Id = exercise.Id,
            Title = exercise.Title,
            Description = exercise.Description,
            ImagePath = exercise.ImagePath,
            IsArchived = exercise.IsArchived,
            MuscleGroups = exercise.MuscleGroups.Select(m => m.ToResponse()).ToList()
        };
    }
    public static SessionResponse ToResponse(this Session session)
    {
        return new SessionResponse(session.Id, session.CreatedAt, session.ExpiresAt);
    }
}