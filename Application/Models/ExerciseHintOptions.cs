namespace Application.Models;

public record ExerciseHintOptions
{
    public int MinRepeats { get; init; }
    public int MaxRepeats{ get; init; }

    public void Validate()
    {
        if (MinRepeats <= 0 || MaxRepeats < MinRepeats)
            throw new InvalidOperationException("Exercise hint repeat limits must satisfy 0 < MinRepeats <= MaxRepeats.");
    }
};
