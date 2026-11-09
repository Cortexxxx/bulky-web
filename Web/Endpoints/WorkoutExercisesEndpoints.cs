using System.Net;
using Application.Services;
using Web.Http;

namespace Web.Endpoints;

public class WorkoutExercisesEndpoints(WorkoutExerciseService service)
{
    public async Task CreateAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var exerciseId = await context.Request.ReadJsonAsync<Guid>(ct);
        var workoutExerciseId = await service.CreateAsync(context.Request.RequiredUserClaims.Id, exerciseId, ct);
        
        context.Response.WriteJson(workoutExerciseId);
        context.Response.StatusCode = HttpStatusCode.Created;
    }
}