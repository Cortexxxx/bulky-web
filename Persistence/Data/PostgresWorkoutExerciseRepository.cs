using Application;
using Application.Abstractions;
using Domain.Models.Workouts;
using Npgsql;

namespace Persistence.Data;

public class PostgresWorkoutExerciseRepository(NpgsqlDataSource dataSource) : IWorkoutExerciseRepository
{
    public async Task<Guid> CreateWorkoutExerciseAsync(Guid userId, Guid exerciseId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
            INSERT INTO public.workout_exercises(workout_id, exercise_id, exercise_number)
            SELECT w.id, @exercise_id, COALESCE(MAX(we.exercise_number), 0) + 1
            FROM public.workouts w
            LEFT JOIN public.workout_exercises we ON we.workout_id = w.id
            WHERE w.user_id = @user_id AND w.ended_at IS NULL
            group by w.id
            RETURNING id;
        """, connection);

        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("exercise_id", exerciseId);

        Guid? id;
        
        try
        {
            id = (Guid?)await command.ExecuteScalarAsync(ct);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("unknown_exercise");
        }

        return id ?? throw new NotFoundException("not_active_workout");
    }
    
    public async Task<bool> DeleteWorkoutExerciseAsync(Guid userId, Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
                DELETE FROM public.workout_exercises we
                USING public.workouts w
                WHERE w.id = we.workout_id AND w.user_id = @user_id AND we.id = @id
                """, connection);
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    public async Task<Guid> CreateSetAsync(Guid userId, Guid workoutExerciseId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO public.sets(workout_exercise_id, set_number)
            SELECT @workout_exercise_id, COALESCE(MAX(s.set_number), 0) + 1
            FROM public.workout_exercises we
            JOIN public.workouts w ON w.id = we.workout_id
            LEFT JOIN public.sets s ON s.workout_exercise_id = we.id
            WHERE w.user_id = @user_id AND w.ended_at IS NULL AND we.id = @workout_exercise_id
            GROUP BY w.id
            RETURNING id
            """, connection);
        
        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("workout_exercise_id", workoutExerciseId);

        return (Guid)(await command.ExecuteScalarAsync(ct) ?? throw new InvalidOperationException("Not received UUID"));
    }

    public Task<Guid> UpdateSetAsync(Guid userId, Guid id, int repeats, decimal weight, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> DeleteSetAsync(Guid userId, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}