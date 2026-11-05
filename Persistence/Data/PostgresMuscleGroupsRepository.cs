using System.Data.Common;
using Application;
using Application.Abstractions;
using Domain.Models.Workouts;
using Npgsql;
using NpgsqlTypes;

namespace Persistence.Data;

public class PostgresMuscleGroupsRepository(NpgsqlDataSource dataSource) : IMuscleGroupsRepository
{
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("""
                                                    SELECT * FROM public.muscle_groups 
                                                    WHERE @include_archived OR NOT is_archived ORDER BY title
                                                    """, connection);

        command.Parameters.AddWithValue("include_archived", includeArchived);

        var muscleGroups = new List<MuscleGroup>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        var columns = Columns.From(reader);

        while (await reader.ReadAsync(ct))
        {
            muscleGroups.Add(CreateMuscleGroup(reader, columns));
        }

        return muscleGroups;
    }

    public async Task<MuscleGroup?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT * FROM public.muscle_groups WHERE id = @id AND (@include_archived OR NOT is_archived)", connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("include_archived", includeArchived);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct)) return null;

        return CreateMuscleGroup(reader, Columns.From(reader));
    }

    public async Task<Guid> CreateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "INSERT INTO public.muscle_groups (title, description, image_path) " +
            "VALUES (@title, @description, @image_path) RETURNING id", connection);
        
        AddDetailsParameters(muscleGroup, command);

        object? guidObj;
        
        try
        {
            guidObj = await command.ExecuteScalarAsync(ct);
        }
        catch (PostgresException e) when(e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new MuscleGroupExistsException("Muscle group is already exists", e);
        }
        
        return (Guid)(guidObj ?? throw new InvalidOperationException("Not received UUID"));
    }

    public async Task<bool> UpdateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "UPDATE public.muscle_groups " +
            "SET title = @title, description = @description, image_path = @image_path " +
            "WHERE id = @id", connection);
        
        command.Parameters.AddWithValue("id", muscleGroup.Id);
        AddDetailsParameters(muscleGroup, command);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }
    
    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "UPDATE public.muscle_groups " +
            "SET is_archived = @is_archived WHERE id = @id", connection);
        
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("is_archived", isArchived);

        return await command.ExecuteNonQueryAsync(ct) != 0;
    }
    
    private static MuscleGroup CreateMuscleGroup(DbDataReader reader, Columns columns)
    {
        return new MuscleGroup
        {
            Id = reader.GetGuid(columns.Id),
            Title = reader.GetString(columns.Title),
            Description = reader.GetString(columns.Description),
            ImagePath = !reader.IsDBNull(columns.ImagePath)
                ? reader.GetString(columns.ImagePath) : null,
            IsArchived = reader.GetBoolean(columns.IsArchived)
        };
    }
    
    private static void AddDetailsParameters(MuscleGroup muscleGroup, NpgsqlCommand command)
    {
        command.Parameters.AddWithValue("title", muscleGroup.Title);
        command.Parameters.AddWithValue("description", muscleGroup.Description);
        
        command.Parameters.Add(new NpgsqlParameter("image_path", NpgsqlDbType.Varchar)
        {
            Value = (object?)muscleGroup.ImagePath ?? DBNull.Value
        });
    }

    private readonly record struct Columns(int Id, int Title, int Description, int ImagePath, int IsArchived)
    {
        public static Columns From(DbDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("title"),
            reader.GetOrdinal("description"),
            reader.GetOrdinal("image_path"),
            reader.GetOrdinal("is_archived"));
    }
}