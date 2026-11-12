using Application.Models;

namespace Application.Abstractions;

public interface IAnalyticsQueries
{
    Task<List<WorkoutWeekStats>> GetWorkoutWeeksStats(Guid userId, CancellationToken ct);
    Task<List<MuscleWeekStats>> GetMuscleWeekStats(Guid userId, Guid muscleId, CancellationToken ct);
}