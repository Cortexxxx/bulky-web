using System.Net;
using Application.Services;
using Domain.Models.Workouts;
using Web.Presentation.DTOs;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Validators;
using ValidationException = FluentValidation.ValidationException;

namespace Web.Presentation.Endpoints;

public class AdminEndpoints(AdminService adminService, 
    MuscleGroupRequestValidator muscleGroupRequestValidator)
{
    public async Task GetAllMuscleGroupsEndpoint(HttpContext context)
    {
        var muscleGroups = await adminService.GetAllMuscleGroupsAsync(context.CancellationToken);
        context.Response.WriteJson(muscleGroups);
    }

    public async Task GetMuscleGroupByIdEndpoint(HttpContext context)
    {
        var id = GetIdOrThrowNotFound(context);

        var muscleGroup = await adminService.GetMuscleGroupByIdAsync(id,
            context.CancellationToken);

        if (muscleGroup is null) ExceptionsHelpers.ThrowNotFound();

        context.Response.WriteJson(muscleGroup);
    }

    public async Task CreateMuscleGroupEndpoint(HttpContext context)
    {
        var muscleGroup = await ReadMuscleGroupAsync(context);
        await ValidateMuscleGroupAsync(context, muscleGroup);
        
        var id = await adminService.CreateMuscleGroupAsync(new MuscleGroup
        {
            Title = muscleGroup.Title,
            Description = muscleGroup.Description,
            ImagePath = null
        }, context.CancellationToken);
            
        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.WriteJson(id);
    }

    public async Task UpdateMuscleGroupEndpoint(HttpContext context)
    {
        var id = GetIdOrThrowNotFound(context);

        var muscleGroup = await ReadMuscleGroupAsync(context);
        await ValidateMuscleGroupAsync(context, muscleGroup);

        var found = await adminService.UpdateMuscleGroupAsync(new MuscleGroup
        {
            Id = id,
            Title = muscleGroup.Title,
            Description = muscleGroup.Description,
            ImagePath = null
        }, context.CancellationToken);

        if (!found) ExceptionsHelpers.ThrowNotFound();

        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.SetContentLength(0);
    }

    public async Task ArchiveMuscleGroupEndpoint(HttpContext context)
    {
        await SetArchive(context, true);
    }
    
    public async Task UnarchiveMuscleGroupEndpoint(HttpContext context)
    {
        await SetArchive(context, false);
    }

    private async Task SetArchive(HttpContext context, bool isArchived)
    {
        var id = GetIdOrThrowNotFound(context);

        var found = await adminService.SetArchivedMuscleGroupAsync(id, isArchived, context.CancellationToken);
        
        if (found)
        {
            context.Response.StatusCode = HttpStatusCode.OK;
            context.Response.SetContentLength(0);
        }
        else
        {
            ExceptionsHelpers.ThrowNotFound();
        }
    }

    private static Guid GetIdOrThrowNotFound(HttpContext context)
    {
        if (!Router.Router.TryGetGuidParameter(context.Request.RouteParameters,
                "id", out var id))
        {
            ExceptionsHelpers.ThrowNotFound();
        }

        return id.GetValueOrDefault();
    }

    private async Task ValidateMuscleGroupAsync(HttpContext context, MuscleGroupRequest muscleGroup)
    {
        var validationResult = await muscleGroupRequestValidator.ValidateAsync(muscleGroup, 
            context.CancellationToken);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException("Incorrect muscle group request", validationResult.Errors);
        }
    }
    
    private static async Task<MuscleGroupRequest> ReadMuscleGroupAsync(HttpContext context)
    {
        var muscleGroup = await context.Request.ReadJsonAsync<MuscleGroupRequest>(context.CancellationToken);
        return muscleGroup;
    }
}