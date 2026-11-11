using System.Net;
using Application.Services;
using Web.DTOs.Workout;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class WorkoutsEndpoints(WorkoutsService workoutsService, WorkoutRequestValidator validator)
{
    public async Task CreateAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var normalizedRequest = await WebHelpers.ProceedRequestAsync(context,
            validator, request => new WorkoutRequest { Title = request.Title.Trim() }, ct);
        
        var id = await workoutsService.CreateAsync(context.Request.RequiredUserClaims.Id, normalizedRequest.Title, ct);

        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.WriteJson(id);
    }

    public async Task DeleteAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        await workoutsService.DeleteAsync(context.Request.RequiredUserClaims.Id, id, context.CancellationToken);
        context.Response.StatusCode = HttpStatusCode.NoContent;
    }

    public async Task GetCurrentAsync(HttpContext context)
    {
        var workout = await workoutsService.GetCurrentAsync(context.Request.RequiredUserClaims.Id, context.CancellationToken);
        
        if (workout == null)
        {
            context.Response.StatusCode = HttpStatusCode.NoContent;
        }
        else
        {
            context.Response.WriteJson(workout.ToResponse());
        }
    }

    public async Task GetByIdAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        var workout = await workoutsService.GetByIdAsync(context.Request.RequiredUserClaims.Id, id, context.CancellationToken);

        context.Response.WriteJson(workout.ToResponse());
    }

    public async Task GetAllAsync(HttpContext context)
    {
        var workouts = await workoutsService.GetAllAsync(context.Request.RequiredUserClaims.Id, context.CancellationToken);
        context.Response.WriteJson(workouts);
    }

    public async Task EndCurrentAsync(HttpContext context)
    {
        await workoutsService.EndCurrentAsync(context.Request.RequiredUserClaims.Id, context.CancellationToken);
    }
}