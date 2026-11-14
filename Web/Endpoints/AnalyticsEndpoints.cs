using Application.Services;
using Web.Http;

namespace Web.Endpoints;

public class AnalyticsEndpoints(WorkoutsAnalyticsService service)
{
    public async Task GetWorkoutsOverview(HttpContext context)
    {
        var overview = await service.GetWorkoutsOverview(context.Request.RequiredUserClaims.Id, context.CancellationToken);
        
        context.Response.WriteJson(overview.ToResponse());
    }
    
    public async Task GetMuscleGroupOverview(HttpContext context)
    {
        var muscleId = WebHelpers.GetIdOrThrowNotFound(context);
        
        var overview = await service.GetMuscleGroupOverview(context.Request.RequiredUserClaims.Id, muscleId, context.CancellationToken);
        
        context.Response.WriteJson(overview.ToResponse());
    }

    public async Task GetExerciseOverview(HttpContext context)
    {
        var exerciseId = WebHelpers.GetIdOrThrowNotFound(context);

        var overview = await service.GetExerciseOverview(context.Request.RequiredUserClaims.Id, exerciseId,
            context.CancellationToken);
        
        context.Response.WriteJson(overview.ToResponse());
    }
}