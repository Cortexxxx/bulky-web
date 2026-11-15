using Application;
using Application.Models;
using Application.Services;
using Web.Http;

namespace Web.Endpoints;

public class GuidesEndpoints(GuideService service)
{
    public async Task GetGuides(HttpContext context)
    {
        IEnumerable<GuideItem>? items = null;
        var ct = context.CancellationToken;
        if (context.Request.TryGetQuery("category", out var query) 
            && Enum.TryParse(query, true, out GuideCategory category))
        {
            items = category switch
            {
                GuideCategory.Exercise => await service.GetExerciseGuides(ct),
                GuideCategory.MuscleGroup => await service.GetMusclesGuides(ct),
                _ => throw new NotFoundException("unknown_category")
            };
        }
        else
        {
            throw new NotFoundException("unknown_category");
        }

        if (items != null)
        {
            context.Response.WriteJson(items
                .Select(el => el.ToResponse())
                .ToList());
        }
    }
}