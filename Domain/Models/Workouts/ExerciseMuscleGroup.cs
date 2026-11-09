namespace Domain.Models.Workouts;

public record ExerciseMuscleGroup
{
    public Guid MuscleGroupId { get; init; }
    public MuscleGroupRole Role { get; init; }
}
