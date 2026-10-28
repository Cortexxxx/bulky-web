using Bulky.Models;
using Npgsql;
using NpgsqlTypes;

namespace Bulky.Data;

public class PostgresSessionRepository(NpgsqlDataSource dataSource) : ISessionRepository
{
    public async Task<Guid> CreateSessionAsync(Guid userId, string token, DateTime expiresAt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.create_session", connection);

        PostgresHelpers.AddUserId(userId, command);
        command.Parameters.AddWithValue("p_token", NpgsqlDbType.Varchar, token);
        command.Parameters.AddWithValue("p_expires_at", NpgsqlDbType.TimestampTz, expiresAt);
        PostgresHelpers.AddOutputParameter("p_id", NpgsqlDbType.Uuid, command);
        
        return await command.ExecuteScalarAsync(ct) is Guid id ? id : throw new InvalidOperationException("Not received UUID");
    }

    public async Task<Session?> GetSessionByTokenAsync(string token, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("SELECT * FROM public.get_session_by_token($1)", connection);
        
        command.Parameters.Add(new NpgsqlParameter()
        {
            NpgsqlDbType = NpgsqlDbType.Varchar, Value = token
        });

        await using var reader = await command.ExecuteReaderAsync(ct);
        var value = await reader.ReadAsync(ct);

        return value
            ? new Session
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                Token = reader.GetString(reader.GetOrdinal("token")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("expires_at"))
            }
            : null;
    }

    public async Task<bool> UpdateSessionTimeAsync(Guid id, DateTime expiresAt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.update_session_time", connection);

        command.Parameters.AddWithValue("p_id", NpgsqlDbType.Uuid, id);
        command.Parameters.AddWithValue("p_expires_at", NpgsqlDbType.TimestampTz, expiresAt);
        PostgresHelpers.AddOutputParameter("p_found", NpgsqlDbType.Boolean, command);

        return await PostgresHelpers.ExecuteAndReadFoundAsync(command, ct);
    }

    public async Task<bool> DeleteUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.delete_user_sessions", connection);

        PostgresHelpers.AddUserId(userId, command);
        PostgresHelpers.AddOutputParameter("p_found", NpgsqlDbType.Boolean, command);
        
        return await PostgresHelpers.ExecuteAndReadFoundAsync(command, ct);
    }

    public async Task<bool> DeleteSessionByIdAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.delete_session_by_id", connection);

        command.Parameters.AddWithValue("p_id", NpgsqlDbType.Uuid, id);
        PostgresHelpers.AddOutputParameter("p_found", NpgsqlDbType.Boolean, command);
        
        return await PostgresHelpers.ExecuteAndReadFoundAsync(command, ct);
    }

    public async Task<bool> DeleteExpiredSessionsAsync(CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.delete_expired_sessions", connection);

        PostgresHelpers.AddOutputParameter("p_found", NpgsqlDbType.Boolean, command);
        return await PostgresHelpers.ExecuteAndReadFoundAsync(command, ct);
    }
}