using System.Data.Common;
using Application.Abstractions;
using Domain.Models.Workouts;
using Npgsql;

namespace Persistence.Data;

public class PostgresWorkoutsRepository(NpgsqlDataSource dataSource) : IWorkoutsRepository
{
    public async Task<Guid> CreateAsync(Guid userId, string title, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    INSERT INTO public.workouts(user_id, title)
                                                    VALUES (@user_id, @title)
                                                    RETURNING id;
                                                    """, connection);   

        AddUserId(userId, command);
        command.Parameters.AddWithValue("title", title);

        return (Guid)(await command.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("Not received UUID"));
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    DELETE FROM public.workouts
                                                    WHERE id = @id AND user_id = @user_id;
                                                    """, connection);

        AddUserId(userId, command);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<Workout?> GetCurrentAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    SELECT w.id AS workout_id, w.title AS workout_title, w.started_at, 
                                                           w.ended_at AS ended_at,
                                                           we.id AS workout_exercise_id, e.title AS exercise_title, 
                                                           e.id AS exercise_id, s.id AS set_id, s.repeats, s.weight
                                                    FROM public.workouts w 
                                                    LEFT JOIN public.workout_exercises we ON we.workout_id = w.id
                                                    LEFT JOIN public.exercises e ON we.exercise_id = e.id
                                                    LEFT JOIN public.sets s ON s.workout_exercise_id = we.id
                                                    WHERE w.user_id = @user_id AND w.ended_at IS NULL
                                                    ORDER BY we.exercise_number, s.set_number
                                                    """, connection);

        AddUserId(userId, command);

        await using var reader = await command.ExecuteReaderAsync(ct);

        return await ReadWorkoutWithExercisesAsync(reader, ct);
    }

    public async Task<IEnumerable<Workout>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    SELECT id, title, started_at, ended_at
                                                    FROM public.workouts
                                                    WHERE user_id = @user_id;
                                                    """, connection);

        AddUserId(userId, command);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var columns = WorkoutColumns.From(reader);

        var workouts = new List<Workout>();

        while (await reader.ReadAsync(ct))
        {
            workouts.Add(CreateWorkout(reader, columns));
        }

        return workouts;
    }

    private static void AddUserId(Guid userId, NpgsqlCommand command)
    {
        command.Parameters.AddWithValue("user_id", userId);
    }

    public async Task<bool> EndCurrentAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    UPDATE public.workouts
                                                    SET ended_at = now()
                                                    WHERE user_id = @user_id AND ended_at IS NULL;
                                                    """, connection);

        AddUserId(userId, command);
        
        return await command.ExecuteNonQueryAsync(ct) != 0;
    }
    
    
    private static async Task<Workout?> ReadWorkoutWithExercisesAsync(DbDataReader reader, CancellationToken ct)
    {
        var columns = Columns.From(reader);
        Workout? workout = null;
        while (await reader.ReadAsync(ct))
        {
            workout ??= new Workout
            {
                Id = reader.GetGuid(columns.WorkoutId),
                Title = reader.GetString(columns.WorkoutTitle),
                StartedAt = reader.GetDateTime(columns.StartedAt),
                EndedAt = reader.IsDBNull(columns.EndedAt)
                    ? null
                    : reader.GetDateTime(columns.EndedAt)
            };

            if (reader.IsDBNull(columns.WorkoutExerciseId)) continue;

            if (workout.WorkoutExercises.LastOrDefault()?.Id != reader.GetGuid(columns.WorkoutExerciseId))
            {
                workout.WorkoutExercises.Add(new WorkoutExercise
                {
                    Id = reader.GetGuid(columns.WorkoutExerciseId),
                    ExerciseTitle = reader.GetString(columns.ExerciseTitle),
                    ExerciseId = reader.GetGuid(columns.ExerciseId),
                    Sets = []
                });
            }

            if (!reader.IsDBNull(columns.SetId))
            {
                workout.WorkoutExercises.Last().Sets.Add(new WorkoutSet
                {
                    Id = reader.GetGuid(columns.SetId),
                    Repeats = reader.GetInt16(columns.Repeats),
                    Weight = reader.GetDecimal(columns.Weight)
                });
            }
        }

        return workout;
    }
    
    private static Workout CreateWorkout(DbDataReader reader, WorkoutColumns columns)
    {
        return new Workout
        {
            Id = reader.GetGuid(columns.Id),
            Title = reader.GetString(columns.Title),
            StartedAt = reader.GetDateTime(columns.StartedAt),
            EndedAt = reader.IsDBNull(columns.EndedAt) ? null : reader.GetDateTime(columns.EndedAt)
        };
    }

    private readonly record struct WorkoutColumns(int Id, int Title, int StartedAt, int EndedAt)
    {
        public static WorkoutColumns From(DbDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("title"),
            reader.GetOrdinal("started_at"),
            reader.GetOrdinal("ended_at"));
    }

    private readonly record struct Columns(
        int WorkoutId, int WorkoutTitle, int StartedAt, int EndedAt,
        int WorkoutExerciseId, int ExerciseTitle, int ExerciseId,
        int SetId, int Repeats, int Weight)
    {
        public static Columns From(DbDataReader reader) => new(
            reader.GetOrdinal("workout_id"),
            reader.GetOrdinal("workout_title"),
            reader.GetOrdinal("started_at"),
            reader.GetOrdinal("ended_at"),
            reader.GetOrdinal("workout_exercise_id"),
            reader.GetOrdinal("exercise_title"),
            reader.GetOrdinal("exercise_id"),
            reader.GetOrdinal("set_id"),
            reader.GetOrdinal("repeats"),
            reader.GetOrdinal("weight"));
    }
}