using System.Net;
using Application.Services;
using Domain.Models.Workouts;
using Web.Presentation.DTOs;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Validators;

namespace Web.Presentation.Endpoints;

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

        if (muscleGroup is null) ExceptionsHelpers.ThrowNotFound();

        context.Response.WriteJson(muscleGroup.Value.ToResponse());
    }

    public async Task CreateMuscleGroupAsync(HttpContext context)
    {
        var muscleGroup = await ReadMuscleGroupAsync(context);
        await WebHelpers.ValidateRequestAsync(muscleGroupRequestValidator, muscleGroup,
            "Incorrect muscle group request", context.CancellationToken);

        var id = await musclesService.CreateAsync(new MuscleGroup
        {
            Title = muscleGroup.Title,
            Description = muscleGroup.Description,
            ImagePath = null
        }, context.CancellationToken);
            
        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.WriteJson(id);
    }

    public async Task UpdateMuscleGroupAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        var muscleGroup = await ReadMuscleGroupAsync(context);
        await WebHelpers.ValidateRequestAsync(muscleGroupRequestValidator, muscleGroup,
            "Incorrect muscle group request", context.CancellationToken);

        var found = await musclesService.UpdateAsync(new MuscleGroup
        {
            Id = id,
            Title = muscleGroup.Title,
            Description = muscleGroup.Description,
            ImagePath = null
        }, context.CancellationToken);

        if (!found) ExceptionsHelpers.ThrowNotFound();

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
        var found = await musclesService.SetArchivedAsync(id, isArchived, context.CancellationToken);
        if (!found) ExceptionsHelpers.ThrowNotFound();
    }

    private static async Task<MuscleGroupRequest> ReadMuscleGroupAsync(HttpContext context)
    {
        return await context.Request.ReadJsonAsync<MuscleGroupRequest>(context.CancellationToken);
    }
}