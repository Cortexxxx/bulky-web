using System.Data.Common;
using Application;
using Application.Abstractions;
using Domain.Models.Auth;
using Npgsql;

namespace Persistence.Data;

public class PostgresUserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT * FROM public.users WHERE email = @email", connection);

        command.Parameters.AddWithValue("email", email);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? CreateUser(reader, Columns.From(reader)) : null;
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT * FROM public.users WHERE id = @id", connection);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? CreateUser(reader, Columns.From(reader)) : null;
    }

    public async Task<Guid> CreateUserAsync(string email, string passwordHash, string passwordSalt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "INSERT INTO public.users (email, password_hash, password_salt) " +
            "VALUES (@email, @password_hash, @password_salt) RETURNING id", connection);

        command.Parameters.AddWithValue("email", email);
        command.Parameters.AddWithValue("password_hash", passwordHash);
        command.Parameters.AddWithValue("password_salt", passwordSalt);

        object? guidObj;

        try
        {
            guidObj = await command.ExecuteScalarAsync(ct);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new EmailAlreadyExistsException("Email already exists", e);
        }

        return (Guid)(guidObj ?? throw new InvalidOperationException("Not received UUID"));
    }

    private static User CreateUser(DbDataReader reader, Columns columns)
    {
        return new User
        {
            Id = reader.GetGuid(columns.Id),
            Email = reader.GetString(columns.Email),
            Role = Enum.Parse<Role>(reader.GetString(columns.Role), ignoreCase: true),
            PasswordHash = reader.GetString(columns.PasswordHash),
            PasswordSalt = reader.GetString(columns.PasswordSalt),
            CreatedAt = reader.GetDateTime(columns.CreatedAt)
        };
    }

    private readonly record struct Columns(
        int Id, int Email, int Role, int PasswordHash, int PasswordSalt, int CreatedAt)
    {
        public static Columns From(DbDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("email"),
            reader.GetOrdinal("role"),
            reader.GetOrdinal("password_hash"),
            reader.GetOrdinal("password_salt"),
            reader.GetOrdinal("created_at"));
    }
}
