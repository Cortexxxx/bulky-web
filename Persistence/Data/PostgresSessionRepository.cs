using System.Data.Common;
using Application.Abstractions;
using Domain.Models.Auth;
using Npgsql;
using NpgsqlTypes;

namespace Persistence.Data;

public class PostgresSessionRepository(NpgsqlDataSource dataSource) : ISessionRepository
{
    public async Task<Guid> CreateSessionAsync(Guid userId, string tokenHash, DateTime expiresAt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "INSERT INTO public.sessions (user_id, token_hash, expires_at) " +
            "VALUES (@user_id, @token_hash, @expires_at) RETURNING id", connection);

        command.Parameters.AddWithValue("user_id", userId);
        command.Parameters.AddWithValue("token_hash", tokenHash);
        command.Parameters.AddWithValue("expires_at", NpgsqlDbType.TimestampTz, expiresAt);

        var guidObj = await command.ExecuteScalarAsync(ct);

        return (Guid)(guidObj ?? throw new InvalidOperationException("Not received UUID"));
    }

    public async Task<Session?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT * FROM public.sessions WHERE token_hash = @token_hash AND expires_at > now()", connection);

        command.Parameters.AddWithValue("token_hash", tokenHash);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? CreateSession(reader, Columns.From(reader)) : null;
    }

    public async Task<IEnumerable<Session>> GetUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT * FROM public.sessions WHERE user_id = @user_id AND expires_at > now()", connection);

        command.Parameters.AddWithValue("user_id", userId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var sessions = new List<Session>();
        while (await reader.ReadAsync(ct))
        {
            sessions.Add(CreateSession(reader, Columns.From(reader)));
        }
        
        return sessions;
    }

    public async Task<bool> UpdateSessionTimeAsync(Guid id, DateTime expiresAt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "UPDATE public.sessions SET expires_at = @expires_at WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("expires_at", NpgsqlDbType.TimestampTz, expiresAt);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    public async Task<bool> DeleteUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "DELETE FROM public.sessions WHERE user_id = @user_id", connection);

        command.Parameters.AddWithValue("user_id", userId);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    public async Task<bool> DeleteSessionByIdAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "DELETE FROM public.sessions WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", id);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    public async Task<bool> DeleteExpiredSessionsAsync(CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "DELETE FROM public.sessions WHERE expires_at < now()", connection);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    private static Session CreateSession(DbDataReader reader, Columns columns)
    {
        return new Session
        {
            Id = reader.GetGuid(columns.Id),
            UserId = reader.GetGuid(columns.UserId),
            Token = reader.GetString(columns.TokenHash),
            CreatedAt = reader.GetDateTime(columns.CreatedAt),
            ExpiresAt = reader.GetDateTime(columns.ExpiresAt)
        };
    }

    private readonly record struct Columns(int Id, int UserId, int TokenHash, int CreatedAt, int ExpiresAt)
    {
        public static Columns From(DbDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("user_id"),
            reader.GetOrdinal("token_hash"),
            reader.GetOrdinal("created_at"),
            reader.GetOrdinal("expires_at"));
    }
}
