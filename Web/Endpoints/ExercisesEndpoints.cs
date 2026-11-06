using System.Net;
using Application.Services;
using Web.DTOs.Exercise;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class ExercisesEndpoints(ExercisesService exercisesService, ExerciseRequestValidator exerciseRequestValidator)
{
    public async Task GetAllExercisesAsync(bool includeArchived, HttpContext context)
    {
        var exercises = await exercisesService.GetAllAsync(includeArchived, context.CancellationToken);
        context.Response.WriteJson(exercises.Select(e => e.ToResponse()).ToList());
    }

    public async Task GetExerciseByIdAsync(bool includeArchived, HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);

        var exercise = await exercisesService.GetByIdAsync(includeArchived, id, context.CancellationToken);
        
        context.Response.WriteJson(exercise.ToResponse());
    }
    
    public async Task CreateExerciseAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var normalizedRequest = await ProceedExerciseRequestAsync(context, ct);

        var id = await exercisesService.CreateAsync( normalizedRequest.Title, normalizedRequest.Description, 
            null, normalizedRequest.MuscleGroups.Select(m => m.ToDomain()).ToList(), ct);

        context.Response.WriteJson(id);
        context.Response.StatusCode = HttpStatusCode.Created;
    }

    public async Task UpdateExerciseAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var normalizedRequest = await ProceedExerciseRequestAsync(context, ct);
        var id = WebHelpers.GetIdOrThrowNotFound(context);

        await exercisesService.UpdateAsync(id, normalizedRequest.Title, normalizedRequest.Description, 
            null, normalizedRequest.MuscleGroups.Select(m => m.ToDomain()).ToList(), ct);
    }
    
    public async Task ArchiveExerciseAsync(HttpContext context) => await SetArchiveStatusAsync(context, true);
    
    public async Task UnarchiveExerciseAsync(HttpContext context) => await SetArchiveStatusAsync(context, false);
    
    private async Task SetArchiveStatusAsync(HttpContext context, bool isArchived)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        await exercisesService.SetArchivedAsync(id, isArchived, context.CancellationToken);
    }
    
    private async Task<ExerciseRequest> ProceedExerciseRequestAsync(HttpContext context, CancellationToken ct)
    {
        return await WebHelpers.ProceedRequestAsync<ExerciseRequest, ExerciseRequestValidator>(context,
            exerciseRequestValidator, request => request with { Title = request.Title.Trim(), 
                Description = request.Description.Trim()}, ct);
    }
}