namespace Domain.Models.Workouts;

public record struct ExerciseMuscleGroup
{
    public Guid ExerciseId { get; init; }
    public Guid MuscleGroupId { get; init; }
    public MuscleGroupRole Role { get; init; }
}
