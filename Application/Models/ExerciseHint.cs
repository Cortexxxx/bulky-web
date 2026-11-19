using Domain.Models.Workouts;

namespace Application.Models;

public record ExerciseHint
{
    private const int MinRepeats = 8;
    private const int MaxRepeats = 12;
    private const double HoldPercentage = 0.5f;
    
    public HintStatus Status { get; init; } = HintStatus.InsufficientData;
    public decimal PreviousWeight { get; init; }
    public List<short> LastSetsRepeats { get; init; } = [];

    public ExerciseHint(IEnumerable<WorkoutSet> exerciseSets)
    {
        var setsGroups = exerciseSets.GroupBy(s => s.Weight)
            .Where(g => g.Count() > 1).ToList();
        var hintGroup = setsGroups
            .MaxBy(g => g.Key);

        if (hintGroup == null)
        {
            Status = HintStatus.InsufficientData;
            return;
        }
        
        var setsRepeats = hintGroup.Select(s => s.Repeats).ToList();
        var setsCount = setsRepeats.Count;
        
        var setsBelowMin = setsRepeats.Count(x => x < MinRepeats);
        var allSetsReachedTarget = setsRepeats.Count(x => x >= MaxRepeats);

        if ((double)setsBelowMin / setsCount > HoldPercentage) Status = HintStatus.Decrease;
        else if (allSetsReachedTarget == setsCount) Status = HintStatus.Increase;
        else Status = HintStatus.Hold;
        
        PreviousWeight = hintGroup.Key;
        LastSetsRepeats = setsRepeats;
    }
}