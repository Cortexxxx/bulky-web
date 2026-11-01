using Application.Abstractions;
using Application.Exceptions;
using Domain.Models.Workouts;
using Npgsql;
using NpgsqlTypes;

namespace Web.Persistence.Data;

public class PostgresMuscleGroupsRepository(NpgsqlDataSource dataSource) : IMuscleGroupsRepository
{
    public async Task<IEnumerable<MuscleGroup>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand("SELECT * FROM public.muscle_groups ORDER BY title", connection);

        var muscleGroups = new List<MuscleGroup>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        var imagePathOrdinal = reader.GetOrdinal("image_path");
        
        while (await reader.ReadAsync(ct))
        {
            muscleGroups.Add(new MuscleGroup
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                ImagePath = !reader.IsDBNull(imagePathOrdinal) 
                    ? reader.GetString(imagePathOrdinal) : null,
                IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived"))
            });
        }

        return muscleGroups;
    }

    public async Task<Guid> CreateAsync(MuscleGroup muscleGroup, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            "INSERT INTO public.muscle_groups (title, description, image_path) " +
            "VALUES (@title, @description, @image_path) RETURNING id", connection);
        
        command.Parameters.AddWithValue("title", muscleGroup.Title);
        command.Parameters.AddWithValue("description", muscleGroup.Description);
        
        command.Parameters.Add(new NpgsqlParameter("image_path", NpgsqlDbType.Varchar)
        {
            Value = (object?)muscleGroup.ImagePath ?? DBNull.Value
        });

        object? guidObj;
        
        try
        {
            guidObj = await command.ExecuteScalarAsync(ct);
        }
        catch (PostgresException e) when(e.ErrorCode == 23505)
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
        command.Parameters.AddWithValue("title", muscleGroup.Title);
        command.Parameters.AddWithValue("description", muscleGroup.Description);
        
        command.Parameters.Add(new NpgsqlParameter("image_path", NpgsqlDbType.Varchar)
        {
            Value = (object?)muscleGroup.ImagePath ?? DBNull.Value
        });

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
}