using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Bulky.Data;

public static class PostgresHelpers
{
    public static NpgsqlCommand CreateProcedure(string name, NpgsqlConnection connection) => new(name, connection)
    {
        CommandType = CommandType.StoredProcedure
    };
    
    public static async Task<bool> ExecuteAndReadFoundAsync(NpgsqlCommand command, CancellationToken ct) 
        => await command.ExecuteScalarAsync(ct) is bool found 
            ? found 
            : throw new InvalidOperationException("Not received p_found");
    
    public static void AddUserId(Guid userId, NpgsqlCommand command) 
        => command.Parameters.AddWithValue("p_user_id", NpgsqlDbType.Uuid, userId);

    public static void AddOutputParameter(string name, NpgsqlDbType type, NpgsqlCommand command) =>
        command.Parameters.Add(new NpgsqlParameter(name, type)
        {
            Direction = ParameterDirection.Output
        });
}