namespace Application.Models;

public record ExerciseHintOptions
{
    public int MinReps { get; init; }
    public int TargetReps{ get; init; }

    public void Validate()
    {
        if (MinReps <= 0 || TargetReps < MinReps)
            throw new InvalidOperationException("Exercise hint repeat limits must satisfy 0 < MinRepeats <= MaxRepeats.");
    }
};
