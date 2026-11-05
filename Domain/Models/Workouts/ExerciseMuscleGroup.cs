namespace Domain.Models.Workouts;

public record struct ExerciseMuscleGroup
{
    public Guid MuscleGroupId { get; init; }
    public MuscleGroupRole Role { get; init; }
}
