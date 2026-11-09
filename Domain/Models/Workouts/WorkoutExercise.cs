namespace Domain.Models.Workouts;

public record WorkoutExercise
{
    public required Guid Id { get; set; }
    public required string ExerciseTitle { get; set; }
    public required Guid ExerciseId { get; set; }
    public List<WorkoutSet> Sets { get; set; } = [];
}