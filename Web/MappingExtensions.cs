using Domain.Models.Workouts;
using Web.DTOs;

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
}