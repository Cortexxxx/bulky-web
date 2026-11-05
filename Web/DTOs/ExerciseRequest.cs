using Domain.Models.Workouts;

namespace Web.DTOs;

public record ExerciseRequest(string Title, string Description, List<ExerciseMuscle> MuscleGroups);