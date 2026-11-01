using System.Net;
using Application.Services;
using Domain.Models.Workouts;
using Web.Presentation.DTOs;
using Web.Presentation.Exceptions;
using Web.Presentation.Http;
using Web.Presentation.Validators;
using ValidationException = FluentValidation.ValidationException;

namespace Web.Presentation.Endpoints;

// TODO: Refactor, remove boilerplate code, add helpers

public class AdminEndpoints(AdminService adminService, 
    MuscleGroupRequestValidator muscleGroupRequestValidator)
{
    public async Task GetAllMuscleGroupsEndpoint(HttpContext context)
    {
        var muscleGroups = await adminService.GetAllMuscleGroupsAsync(context.CancellationToken);
        context.Response.WriteJson(muscleGroups);
    }

    public async Task CreateMuscleGroupEndpoint(HttpContext context)
    {
        var request = context.Request;
        var muscleGroup = await request.ReadJsonAsync<MuscleGroupRequest>(context.CancellationToken);
        var validationResult = await muscleGroupRequestValidator.ValidateAsync(muscleGroup, context.CancellationToken);
        
        if (validationResult.IsValid)
        {
            var id = await adminService.CreateMuscleGroupAsync(new MuscleGroup
            {
                Title = muscleGroup.Title,
                Description = muscleGroup.Description,
                ImagePath = null
            }, context.CancellationToken);
            
            context.Response.StatusCode = HttpStatusCode.Created;
            context.Response.WriteJson(id);
        }
        else
        {
            throw new ValidationException("Incorrect muscle group request", validationResult.Errors);
        }
    }

    public async Task UpdateMuscleGroupEndpoint(HttpContext context)
    {
        var request = context.Request;
        var muscleGroup = await request.ReadJsonAsync<MuscleGroupRequest>(context.CancellationToken);
        
        var validationResult = await muscleGroupRequestValidator.ValidateAsync(muscleGroup, 
            context.CancellationToken);
        var isValidParam = Router.Router.TryGetGuidParameter(request.RouteParameters, 
            "id", out var id);
        
        if (!isValidParam)
        {
            ExceptionsHelpers.ThrowNotFound();
        }
        
        if (validationResult.IsValid)
        {
            var found = await adminService.UpdateMuscleGroupAsync(new MuscleGroup
            {
                Id = id.GetValueOrDefault(),
                Title = muscleGroup.Title,
                Description = muscleGroup.Description,
                ImagePath = null
            }, context.CancellationToken);

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
        else
        {
            throw new ValidationException("Incorrect muscle group request", validationResult.Errors);
        }
    }

    public async Task ArchiveMuscleGroupEndpoint(HttpContext context)
    {
        var isValidParam = Router.Router.TryGetGuidParameter(context.Request.RouteParameters, 
            "id", out var id);
        
        if (!isValidParam)
        {
            ExceptionsHelpers.ThrowNotFound();
        }
        
        var found = await adminService.SetArchivedMuscleGroupAsync(id.GetValueOrDefault(), true, context.CancellationToken);
        
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
    
    public async Task UnarchiveMuscleGroupEndpoint(HttpContext context)
    {
        var isValidParam = Router.Router.TryGetGuidParameter(context.Request.RouteParameters, 
            "id", out var id);
        
        if (!isValidParam)
        {
            ExceptionsHelpers.ThrowNotFound();
        }
        
        var found = await adminService.SetArchivedMuscleGroupAsync(id.GetValueOrDefault(), false, context.CancellationToken);
        
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
}