using System.Net;
using Application.Services;
using Domain.Models.Workouts;
using Web.DTOs.MuscleGroup;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class MusclesEndpoints(MusclesService musclesService, 
    MuscleGroupRequestValidator muscleGroupRequestValidator)
{
    public async Task GetAllMuscleGroupsAsync(bool includeArchived, HttpContext context)
    {
        var muscleGroups = await musclesService.GetAllAsync(includeArchived, context.CancellationToken);
        context.Response.WriteJson(muscleGroups.Select(mg => mg.ToResponse()));
    }

    public async Task GetMuscleGroupByIdAsync(bool includeArchived, HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);

        var muscleGroup = await musclesService.GetByIdAsync(includeArchived, id,
            context.CancellationToken);
        
        context.Response.WriteJson(muscleGroup.ToResponse());
    }

    public async Task CreateMuscleGroupAsync(HttpContext context)
    {
        var normalizedRequest = await ProceedMuscleRequestAsync(context);

        var id = await musclesService.CreateAsync(new MuscleGroup
        {
            Title = normalizedRequest.Title,
            Description = normalizedRequest.Description,
            ImagePath = null
        }, context.CancellationToken);
            
        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.WriteJson(id);
    }

    public async Task UpdateMuscleGroupAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        var normalizedRequest = await ProceedMuscleRequestAsync(context);

        await musclesService.UpdateAsync(new MuscleGroup
        {
            Id = id,
            Title = normalizedRequest.Title,
            Description = normalizedRequest.Description,
            ImagePath = null
        }, context.CancellationToken);
    }

    public async Task ArchiveMuscleGroupAsync(HttpContext context)
    {
        await SetArchiveAsync(context, true);
    }
    
    public async Task UnarchiveMuscleGroupAsync(HttpContext context)
    {
        await SetArchiveAsync(context, false);
    }

    private async Task SetArchiveAsync(HttpContext context, bool isArchived)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        await musclesService.SetArchivedAsync(id, isArchived, context.CancellationToken);
    }
    
    private async Task<MuscleGroupRequest> ProceedMuscleRequestAsync(HttpContext context)
    {
        return await WebHelpers.ProceedRequestAsync<MuscleGroupRequest, MuscleGroupRequestValidator>(context,
            muscleGroupRequestValidator, request => 
                new MuscleGroupRequest(request.Title.Trim(), request.Description.Trim()), context.CancellationToken);
    }
}