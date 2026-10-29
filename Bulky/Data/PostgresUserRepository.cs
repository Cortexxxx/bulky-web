using Bulky.Exceptions;
using Bulky.Http;
using Bulky.Models;
using Npgsql;
using NpgsqlTypes;

namespace Bulky.Data;

public class PostgresUserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("SELECT * FROM public.get_user_by_email($1)", connection);

        command.Parameters.Add(new NpgsqlParameter()
        {
            NpgsqlDbType = NpgsqlDbType.Varchar, Value = email
        });

        
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)
            ? new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                Role = WebHelpers.GetRoleByString(reader.GetString(reader.GetOrdinal("role"))),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                PasswordSalt = reader.GetString(reader.GetOrdinal("password_salt")),
                CreatedAt =  reader.GetDateTime(reader.GetOrdinal("created_at"))
            }
            : null;
    }

    public async Task<Guid> CreateUserAsync(string email, string passwordHash, string passwordSalt, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = PostgresHelpers.CreateProcedure("public.create_user", connection);

        command.Parameters.AddWithValue("p_email", email);
        command.Parameters.AddWithValue("p_password_hash", passwordHash);
        command.Parameters.AddWithValue("p_password_salt", passwordSalt);
        
        PostgresHelpers.AddOutputParameter("p_id", NpgsqlDbType.Uuid, command);
        
        try
        {
            var idObj = await command.ExecuteScalarAsync(ct);
            
            if (idObj is Guid guid)
            {
                return guid;
            }

            throw new InvalidOperationException("Not received UUID");
        }
        catch (PostgresException e) when (e.SqlState == "23505")
        {
            throw new EmailAlreadyExistsException("Email already exists", e);
        }
    }
}