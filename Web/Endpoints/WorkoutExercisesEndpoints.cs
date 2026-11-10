using System.Net;
using Application.Services;
using Web.Http;
using Web.Routing;

namespace Web.Endpoints;

public class WorkoutExercisesEndpoints(WorkoutExerciseService service)
{
    public async Task CreateExerciseAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var exerciseId = await context.Request.ReadJsonAsync<Guid>(ct);
        var workoutExerciseId = await service.CreateExerciseAsync(context.Request.RequiredUserClaims.Id, exerciseId, ct);
        
        context.Response.WriteJson(workoutExerciseId);
        context.Response.StatusCode = HttpStatusCode.Created;
    }

    public async Task DeleteExerciseAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);

        await service.DeleteExerciseAsync(context.Request.RequiredUserClaims.Id, id, context.CancellationToken);
    }

    public async Task CreateSetAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var id = await service.CreateSetAsync(context.Request.RequiredUserClaims.Id,
            await context.Request.ReadJsonAsync<Guid>(ct), ct);
        
        context.Response.WriteJson(id);
        context.Response.StatusCode = HttpStatusCode.Created;
    }

    public async Task DeleteSetAsync(HttpContext context)
    {
        var id = WebHelpers.GetIdOrThrowNotFound(context);
        await service.DeleteSetAsync(context.Request.RequiredUserClaims.Id, id, context.CancellationToken);
    }
}