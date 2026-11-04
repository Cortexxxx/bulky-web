using Domain.Models.Workouts;

namespace Web.Presentation.DTOs;

public record ExerciseRequest(string Title, string Description, List<ExerciseMuscle> MuscleGroups);