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
                WHERE w.user_id = @user_id AND w.ended_at IS NOT NULL AND now() - w.started_at < interval '12 week'
                GROUP BY week_number
            ),
            weekly_sets AS (
                SELECT floor(extract(epoch from now() - w.started_at) / extract(epoch from interval '1 week'))::int AS week_number,
                       sum(s.repeats * s.weight) AS total_volume, count(*) AS total_sets
                FROM public.workouts w
                         JOIN public.workout_exercises we ON w.id = we.workout_id
                         JOIN public.sets s ON we.id = s.workout_exercise_id
                WHERE w.user_id = @user_id AND w.ended_at IS NOT NULL AND now() - started_at < interval '12 week' AND s.repeats != 0 AND s.weight != 0
                GROUP BY week_number
            )
            SELECT n AS week_number, COALESCE(w.workouts_count, 0) AS workouts_count, COALESCE(w.total_time, interval '0') AS total_time, COALESCE(s.total_volume, 0) AS total_volume, COALESCE(s.total_sets, 0) AS total_sets
            FROM generate_series(0,11) AS n
                     LEFT JOIN weekly_workouts w ON w.week_number = n
                     LEFT JOIN weekly_sets s ON s.week_number = n
            
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

    public async Task<List<MuscleWeekStats>> GetMuscleWeekStats(Guid userId, Guid muscleId, CancellationToken ct)
    { 
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            WITH weekly_stats AS (
                SELECT floor(extract(epoch from now() - w.started_at) / extract(epoch from interval '1 week'))::int AS week_number,
                       sum(s.repeats * s.weight * CASE emg.role WHEN 'primary' THEN 1 ELSE 0.5 END)::int AS total_volume, 
                       count(DISTINCT w.id)::int AS total_workouts,
                       sum(CASE emg.role WHEN 'primary' THEN 1 ELSE 0.5 END) AS total_sets
                FROM public.workouts w
                JOIN public.workout_exercises we on w.id = we.workout_id
                JOIN public.sets s on we.id = s.workout_exercise_id
                JOIN public.exercise_muscle_groups emg ON we.exercise_id = emg.exercise_id
                WHERE w.user_id = @user_id AND emg.muscle_group_id = @muscle_group_id AND w.ended_at IS NOT NULL 
                  AND s.repeats != 0 AND s.weight != 0 AND now() - w.started_at < interval '12 week'
                GROUP BY week_number
            )
            SELECT n AS week_number, COALESCE(w.total_volume, 0) AS total_volume, COALESCE(w.total_sets, 0) AS total_sets, 
                   COALESCE(w.total_workouts, 0) AS total_workouts
            FROM generate_series(0,11) AS n
                     LEFT JOIN weekly_stats w ON w.week_number = n
            ORDER BY n
            """, connection);

        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("muscle_group_id", muscleId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var stats = new List<MuscleWeekStats>();
        
        while (await reader.ReadAsync(ct))
        {
            stats.Add(new MuscleWeekStats
            {
                WeekNumber = reader.GetInt32(reader.GetOrdinal("week_number")),
                TotalVolume = reader.GetInt32(reader.GetOrdinal("total_volume")),
                TotalSets = reader.GetInt32(reader.GetOrdinal("total_sets")),
            });
        }

        return stats;
    }

    public async Task<List<ExerciseWeekSet>> GetExerciseWeekSets(Guid userId, Guid exerciseId, CancellationToken ct)
    { 
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            SELECT floor(extract(epoch from now() - w.started_at) / extract(epoch from interval '1 week'))::int AS week_number,
                   s.weight AS weight, s.repeats as repeats
            FROM public.workouts w
                     JOIN public.workout_exercises we on w.id = we.workout_id
                     JOIN public.sets s on we.id = s.workout_exercise_id
                     JOIN public.exercises e ON we.exercise_id = e.id
            WHERE w.user_id = @user_id AND e.id = @exercise_id AND w.ended_at IS NOT NULL
              AND s.repeats != 0 AND s.weight != 0 AND now() - w.started_at < interval '12 week'
            ORDER BY week_number
            """, connection);

        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("exercise_id", exerciseId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var stats = new List<ExerciseWeekSet>();
        
        while (await reader.ReadAsync(ct))
        {
            stats.Add(new ExerciseWeekSet
            {
                WeekNumber = reader.GetInt32(reader.GetOrdinal("week_number")),
                Repeats = reader.GetInt32(reader.GetOrdinal("repeats")),
                Weight = reader.GetDecimal(reader.GetOrdinal("weight"))
            });
        }

        return stats;
    }
}