using Domain.Models.Workouts;

namespace Web.DTOs.Exercise;

public record ExerciseMuscleDto(Guid MuscleGroupId, MuscleGroupRole Role);