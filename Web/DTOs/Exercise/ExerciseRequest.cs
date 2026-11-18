namespace Web.DTOs.Exercise;

public record ExerciseRequest(string Title, string Description, string? ImagePath, List<ExerciseMuscleDto> MuscleGroups);