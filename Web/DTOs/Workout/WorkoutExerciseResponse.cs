namespace Web.DTOs.Workout;

public record WorkoutExerciseResponse
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseTitle { get; init; } = "";
    public List<WorkoutSetResponse> Sets { get; init; } = [];
}
