namespace Web.DTOs.Exercise;

public record ExerciseRequest(string Title, string Description, List<ExerciseMuscleDto> MuscleGroups);