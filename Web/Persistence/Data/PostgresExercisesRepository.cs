using System.Data;
using System.Data.Common;
using Application.Abstractions;
using Domain.Models.Workouts;
using Npgsql;
using NpgsqlTypes;

namespace Web.Persistence.Data;

public class PostgresExercisesRepository(NpgsqlDataSource dataSource) : IExercisesRepository
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool showArchived, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            SELECT exercise.*, exercise_muscle_group.muscle_group_id as muscle_group_id, exercise_muscle_group.role
            FROM public.exercises as exercise
            LEFT JOIN public.exercise_muscle_groups exercise_muscle_group on exercise.id = exercise_muscle_group.exercise_id
            ORDER BY exercise.title;
            """, connection);

        var exercises = new Dictionary<Guid, Exercise>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        
        var columns = Columns.From(reader);

        while (await reader.ReadAsync(ct))
        {
            var exerciseId = reader.GetGuid(columns.Id);
            var group = await ReadMuscleGroupAsync(reader, columns, exerciseId, ct);

            if (exercises.TryGetValue(exerciseId, out var exercise))
            {
                if (group != null)
                {
                    exercise.MuscleGroups.Add(group.Value);
                }
            }
            else
            {
                exercises[exerciseId] = new Exercise
                {
                    Id = exerciseId,
                    Title = reader.GetString(columns.Title),
                    Description = reader.GetString(columns.Description),
                    ImagePath = await reader.IsDBNullAsync(columns.ImagePath, ct)
                        ? null : reader.GetString(columns.ImagePath),
                    IsArchived = reader.GetBoolean(columns.IsArchived),
                    MuscleGroups = group == null ? [] : [group.Value]
                };

            }
        }

        return exercises.Values;
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            SELECT exercise.*, exercise_muscle_group.muscle_group_id as muscle_group_id, exercise_muscle_group.role
            FROM public.exercises as exercise
            LEFT JOIN public.exercise_muscle_groups exercise_muscle_group on exercise.id = exercise_muscle_group.exercise_id
            WHERE exercise.id = @id
            ORDER BY exercise_muscle_group.muscle_group_id;
            """, connection);

        command.Parameters.AddWithValue("id", id);
        
        await using var reader = await command.ExecuteReaderAsync(ct);
        var isRead = await reader.ReadAsync(ct);
        if (!isRead) return null;
        
        var columns = Columns.From(reader);
        var group = await ReadMuscleGroupAsync(reader, columns, id, ct);

        var exercise = new Exercise
        {
            Id = id,
            Title = reader.GetString(columns.Title),
            Description = reader.GetString(columns.Description),
            ImagePath = reader.GetString(columns.ImagePath),
            IsArchived = reader.GetBoolean(columns.IsArchived),
            MuscleGroups = group == null ? [] : [group.Value]
        };

        while (await reader.ReadAsync(ct))
        {
            group = await ReadMuscleGroupAsync(reader, columns, id, ct);
            
            if (group == null)
            {
                continue;
            }
            
            exercise.MuscleGroups.Add(group.Value);
        }

        return exercise;
    }

    public async Task<Guid> CreateAsync(Exercise exercise, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var createExerciseCommand = new NpgsqlCommand(
            """
            INSERT INTO public.exercises(id, title, description, image_path)
            VALUES (@id, @title, @description, @image_path)
            """, connection, transaction);

        var id = exercise.Id;
        createExerciseCommand.Parameters.AddWithValue("id", id);
        createExerciseCommand.Parameters.AddWithValue("title", exercise.Title);
        createExerciseCommand.Parameters.AddWithValue("description", exercise.Description);
        createExerciseCommand.Parameters.Add(new NpgsqlParameter("image_path", NpgsqlDbType.Varchar)
        {
            Value = (object?)exercise.ImagePath ?? DBNull.Value
        });
        
        await createExerciseCommand.ExecuteNonQueryAsync(ct);

        await using var createExerciseMuscles = new NpgsqlCommand(
            """
            INSERT INTO public.exercise_muscle_groups(exercise_id, muscle_group_id, role)
            SELECT @exercise_id, m.muscle_id, m.role
            FROM unnest(@muscle_ids, @roles) as m(muscle_id, role)
            """, connection, transaction);

        var muscleIds = exercise.MuscleGroups.Select(mg => mg.MuscleGroupId).ToArray();
        var roles = exercise.MuscleGroups.Select(mg => mg.Role.ToString().ToLowerInvariant()).ToArray();
        
        createExerciseMuscles.Parameters.AddWithValue("exercise_id", id);
        createExerciseMuscles.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "muscle_ids",
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Uuid,
            Value = muscleIds
        });
        
        createExerciseMuscles.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "roles",
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Varchar,
            Value = roles
        });

        await createExerciseMuscles.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        
        return id;
    }

    public Task<bool> UpdateAsync(Exercise muscleGroup, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    private static async Task<ExerciseMuscleGroup?> ReadMuscleGroupAsync(
        DbDataReader reader, Columns columns, Guid exerciseId, CancellationToken ct)
    {
        if (await reader.IsDBNullAsync(columns.MuscleGroupId, ct))
        {
            return null;
        }

        return new ExerciseMuscleGroup
        {
            ExerciseId = exerciseId,
            MuscleGroupId = reader.GetGuid(columns.MuscleGroupId),
            Role = Enum.Parse<MuscleGroupRole>(reader.GetString(columns.Role), ignoreCase: true)
        };
    }

    private readonly record struct Columns(
        int Id, int Title, int Description, int ImagePath, int IsArchived, int MuscleGroupId, int Role)
    {
        public static Columns From(DbDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("title"),
            reader.GetOrdinal("description"),
            reader.GetOrdinal("image_path"),
            reader.GetOrdinal("is_archived"),
            reader.GetOrdinal("muscle_group_id"),
            reader.GetOrdinal("role"));
    }
}