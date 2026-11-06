using System.Net;
using Application.Services;
using Web.DTOs;
using Web.Exceptions;
using Web.Http;
using Web.Validators;

namespace Web.Endpoints;

public class WorkoutsEndpoints(WorkoutsService workoutsService, WorkoutRequestValidator validator)
{
    public async Task CreateAsync(HttpContext context)
    {
        var ct = context.CancellationToken;
        var normalizedRequest = await WebHelpers.ProceedRequestAsync<WorkoutRequest, WorkoutRequestValidator>(context,
            validator, request => new WorkoutRequest { Title = request.Title.Trim() }, ct);
        
        var id = await workoutsService.CreateAsync(context.Request.RequiredUserClaims.Id, normalizedRequest.Title, ct);

        context.Response.StatusCode = HttpStatusCode.Created;
        context.Response.WriteJson(id);
    }

    public async Task DeleteAsync(HttpContext context)
    {
        
    }

    public async Task GetCurrentAsync(HttpContext context)
    {

    }

    public async Task GetAllAsync(HttpContext context)
    {

    }

    public async Task EndCurrentAsync(HttpContext context)
    {

    }
}