using System.Data.Common;
using Application.Abstractions;
using Application.Exceptions;
using Domain.Models.Workouts;
using Npgsql;
using NpgsqlTypes;

namespace Web.Persistence.Data;

public class PostgresExercisesRepository(NpgsqlDataSource dataSource) : IExercisesRepository
{
    public async Task<IEnumerable<Exercise>> GetAllAsync(bool includeArchived, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            SELECT exercise.*, exercise_muscle_group.muscle_group_id as muscle_group_id, exercise_muscle_group.role
            FROM public.exercises as exercise
            LEFT JOIN public.exercise_muscle_groups exercise_muscle_group on exercise.id = exercise_muscle_group.exercise_id
            WHERE @include_archived OR NOT exercise.is_archived
            ORDER BY exercise.title;
            """, connection);

        command.Parameters.AddWithValue("include_archived", includeArchived);

        var exercises = new Dictionary<Guid, Exercise>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        
        var columns = Columns.From(reader);

        while (await reader.ReadAsync(ct))
        {
            var exerciseId = reader.GetGuid(columns.Id);
            var group = ReadMuscleGroupAsync(reader, columns, exerciseId);

            if (exercises.TryGetValue(exerciseId, out var exercise))
            {
                if (group != null)
                {
                    exercise.MuscleGroups.Add(group.Value);
                }
            }
            else
            {
                exercises[exerciseId] = CreateExercise(reader, columns, group, exerciseId);
            }
        }

        return exercises.Values;
    }

    public async Task<Exercise?> GetByIdAsync(bool includeArchived, Guid id, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            SELECT exercise.*, exercise_muscle_group.muscle_group_id as muscle_group_id, exercise_muscle_group.role
            FROM public.exercises as exercise
            LEFT JOIN public.exercise_muscle_groups exercise_muscle_group on exercise.id = exercise_muscle_group.exercise_id
            WHERE exercise.id = @id AND (@include_archived OR NOT exercise.is_archived)
            ORDER BY exercise_muscle_group.muscle_group_id;
            """, connection);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("include_archived", includeArchived);
        
        await using var reader = await command.ExecuteReaderAsync(ct);
        var isRead = await reader.ReadAsync(ct);
        if (!isRead) return null;
        
        var columns = Columns.From(reader);
        var group = ReadMuscleGroupAsync(reader, columns, id);

        var exercise = CreateExercise(reader, columns, group, id);

        while (await reader.ReadAsync(ct))
        {
            group = ReadMuscleGroupAsync(reader, columns, id);
            
            if (group == null)
            {
                continue;
            }
            
            exercise.MuscleGroups.Add(group.Value);
        }

        return exercise;
    }

    public async Task CreateAsync(Exercise exercise, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var createExerciseCommand = new NpgsqlCommand(
            """
            INSERT INTO public.exercises(id, title, description, image_path)
            VALUES (@id, @title, @description, @image_path)
            """, connection, transaction);

        AddExerciseParameters(exercise, createExerciseCommand);

        try
        {
            await createExerciseCommand.ExecuteNonQueryAsync(ct);
        }
        catch (PostgresException e) when(e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ExerciseExistsException("Exercise is already exists!", e);
        }

        await CreateExerciseMuscles(connection, transaction, exercise, ct);
        await transaction.CommitAsync(ct);
        
    }

    public async Task<bool> UpdateAsync(Exercise exercise, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var updateExerciseCommand = new NpgsqlCommand(
            """
            UPDATE public.exercises
            SET (title, description, image_path) = (@title, @description, @image_path)
            WHERE id = @id
            """, connection, transaction);

        var id = exercise.Id;
        AddExerciseParameters(exercise, updateExerciseCommand);

        var found = await updateExerciseCommand.ExecuteNonQueryAsync(ct) != 0;

        await using var deleteAllExerciseMuscles = new NpgsqlCommand(
            "DELETE FROM public.exercise_muscle_groups WHERE exercise_id = @id", connection, transaction);

        deleteAllExerciseMuscles.Parameters.AddWithValue("id", id);
        await deleteAllExerciseMuscles.ExecuteNonQueryAsync(ct);

        await CreateExerciseMuscles(connection, transaction, exercise, ct);
        
        await transaction.CommitAsync(ct);

        return found;
    }

    public async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(
            """
            UPDATE public.exercises
            SET is_archived = @is_archived 
            WHERE id = @id
            """, connection);

        command.Parameters.AddWithValue("is_archived", isArchived);
        command.Parameters.AddWithValue("id", id);
        
        return await command.ExecuteNonQueryAsync(ct) != 0;
    }

    private static ExerciseMuscleGroup? ReadMuscleGroupAsync(
        DbDataReader reader, Columns columns, Guid exerciseId)
    {
        return reader.IsDBNull(columns.MuscleGroupId) ? null : new ExerciseMuscleGroup
        {
            ExerciseId = exerciseId,
            MuscleGroupId = reader.GetGuid(columns.MuscleGroupId),
            Role = Enum.Parse<MuscleGroupRole>(reader.GetString(columns.Role), ignoreCase: true)
        };
    }
    
    private static async Task CreateExerciseMuscles(NpgsqlConnection connection, NpgsqlTransaction transaction,
        Exercise exercise, CancellationToken ct)
    {
        await using var createExerciseMuscles = new NpgsqlCommand(
            """
            INSERT INTO public.exercise_muscle_groups(exercise_id, muscle_group_id, role)
            SELECT @exercise_id, m.muscle_id, m.role
            FROM unnest(@muscle_ids, @roles) as m(muscle_id, role)
            """, connection, transaction);
        
        var muscleIds = exercise.MuscleGroups.Select(mg => mg.MuscleGroupId).ToArray();
        var roles = exercise.MuscleGroups.Select(mg => mg.Role.ToString().ToLowerInvariant()).ToArray();
        
        createExerciseMuscles.Parameters.AddWithValue("exercise_id", exercise.Id);
        createExerciseMuscles.Parameters.Add(new NpgsqlParameter("muscle_ids", 
            NpgsqlDbType.Array | NpgsqlDbType.Uuid)
        {
            Value = muscleIds
        });
        
        createExerciseMuscles.Parameters.Add(new NpgsqlParameter("roles", 
            NpgsqlDbType.Array | NpgsqlDbType.Varchar)
        {
            Value = roles
        });
        
        try
        {
            await createExerciseMuscles.ExecuteNonQueryAsync(ct);
        }
        catch (PostgresException e) when(e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new UnknownMuscleGroupException("Unknown muscle group!", e);
        }

    }
    
    private static Exercise CreateExercise(NpgsqlDataReader reader, 
        Columns columns, ExerciseMuscleGroup? group, Guid exerciseId)
    {
        return new Exercise
        {
            Id = exerciseId,
            Title = reader.GetString(columns.Title),
            Description = reader.GetString(columns.Description),
            ImagePath = reader.IsDBNull(columns.ImagePath)
                ? null : reader.GetString(columns.ImagePath),
            IsArchived = reader.GetBoolean(columns.IsArchived),
            MuscleGroups = group == null ? [] : [group.Value]
        };
    }
    
    private static void AddExerciseParameters(Exercise exercise, NpgsqlCommand command)
    {
        command.Parameters.AddWithValue("id", exercise.Id);
        command.Parameters.AddWithValue("title", exercise.Title);
        command.Parameters.AddWithValue("description", exercise.Description);
        command.Parameters.Add(new NpgsqlParameter("image_path", NpgsqlDbType.Varchar)
        {
            Value = (object?)exercise.ImagePath ?? DBNull.Value
        });
    }
    
    private readonly record struct Columns(
        int Id, int Title, int Description, int ImagePath, int IsArchived, int MuscleGroupId, int Role)
    {
        public static Columns From(NpgsqlDataReader reader) => new(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("title"),
            reader.GetOrdinal("description"),
            reader.GetOrdinal("image_path"),
            reader.GetOrdinal("is_archived"),
            reader.GetOrdinal("muscle_group_id"),
            reader.GetOrdinal("role"));
    }
}