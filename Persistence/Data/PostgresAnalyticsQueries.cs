using Application.Abstractions;
using Application.Models;
using Npgsql;

namespace Persistence.Data;

public class PostgresAnalyticsQueries(NpgsqlDataSource dataSource) : IAnalyticsQueries
{
    public async Task<List<WorkoutWeekStats>> GetWorkoutWeeksStats(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            WITH weekly_workouts AS (
                SELECT floor(extract(epoch from now() - w.started_at) / extract(epoch from interval '1 week'))::int AS week_number,
                       count(w) AS workouts_count, sum(w.ended_at - w.started_at) AS total_time
                FROM public.workouts w
                WHERE w.user_id = @user_id AND w.ended_at IS NOT NULL AND now() - started_at < interval '12 week'
                GROUP BY week_number
            )
            WITH weekly_sets AS (
                SELECT floor(extract(epoch from now() - w.started_at) / extract(epoch from interval '1 week'))::int AS week_number,
                       sum(s.repeats * s.weight) AS total_volume, count(*) AS total_sets
                FROM public.workouts w
                JOIN public.workout_exercises we ON w.id = we.workout_id
                JOIN public.sets s ON we.id = s.workout_exercise_id
                WHERE w.user_id = @user_id AND w.ended_at IS NOT NULL AND now() - started_at < interval '12 week' AND s.repeats != 0 AND s.weight != 0
                GROUP BY week_number
            )
            SELECT n AS week_number, COALESCE(w.workouts_count, 0), COALESCE(w.total_time, 0), COALESCE(s.total_volume, 0), COALESCE(s.total_sets, 0)
            FROM generate_series(0,11) AS n
                     LEFT JOIN weekly_workouts w ON w.week_number = n
                     LEFT JOIN weekly_sets s ON w.week_number = n
            
            ORDER BY n
            """, connection);

        command.Parameters.AddWithValue("user_id", userId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var stats = new List<WorkoutWeekStats>();
        
        while (await reader.ReadAsync(ct))
        {
            stats.Add(new WorkoutWeekStats
            {
                WeekNumber = reader.GetInt32(reader.GetOrdinal("week_number")),
                WorkoutsCount = reader.GetInt32(reader.GetOrdinal("workouts_count")),
                TotalTime = reader.GetTimeSpan(reader.GetOrdinal("total_time")),
                TotalSets = reader.GetInt32(reader.GetOrdinal("total_sets")),
                TotalVolume = reader.GetInt32(reader.GetOrdinal("total_volume"))
            });
        }

        return stats;
    }


}