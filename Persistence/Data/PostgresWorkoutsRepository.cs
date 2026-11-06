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
                                                    SELECT * FROM public.workouts
                                                    WHERE user_id = @user_id AND ended_at IS NULL;
                                                    """, connection);

        AddUserId(userId, command);

        await using var reader = await command.ExecuteReaderAsync(ct);

        return await reader.ReadAsync(ct) ? CreateWorkout(reader) : null;
    }

    public async Task<IEnumerable<Workout>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    SELECT * FROM public.workouts
                                                    WHERE user_id = @user_id;
                                                    """, connection);
        
        AddUserId(userId, command);

        await using var reader = await command.ExecuteReaderAsync(ct);

        var workouts = new List<Workout>();
        
        while (await reader.ReadAsync(ct))
        {
            workouts.Add(CreateWorkout(reader));
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
    
    private static Workout CreateWorkout(DbDataReader reader)
    {
        return new Workout
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetGuid(1),
            Title = reader.GetString(2),
            StartedAt = reader.GetDateTime(3),
            EndedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
        };
    }
}