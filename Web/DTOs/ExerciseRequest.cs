namespace Web.DTOs;

public record ExerciseRequest(string Title, string Description, List<ExerciseMuscleDto> MuscleGroups);