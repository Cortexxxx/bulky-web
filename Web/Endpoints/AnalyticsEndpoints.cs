using Application.Services;
using Web.Http;

namespace Web.Endpoints;

public class AnalyticsEndpoints(WorkoutsAnalyticsService service)
{
    public async Task GetOverview(HttpContext context)
    {
        var overview = await service.GetWorkoutsOverview(context.Request.RequiredUserClaims.Id, context.CancellationToken);
        
        context.Response.WriteJson(overview.ToResponse());
    }
}