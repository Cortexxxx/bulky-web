using Domain.Models.Workouts;

namespace Web.DTOs;

public record ExerciseMuscleDto(Guid MuscleGroupId, MuscleGroupRole Role);