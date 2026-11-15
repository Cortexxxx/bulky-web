using Application.Models;

namespace Application.Services;

public class GuideService(ExercisesService exercisesService, MusclesService musclesService)
{
    public async Task<List<GuideItem>?> GetExerciseGuides(CancellationToken ct)
    {
        return (await exercisesService.GetAllAsync(false, ct)).Select(e => new GuideItem
        {
            Category = GuideCategory.Exercise,
            SourceId = e.Id,
            Title = e.Title,
            Description = e.Description,
            ImagePath = e.ImagePath
        }).ToList();
    }

    public async Task<List<GuideItem>> GetMusclesGuides(CancellationToken ct)
    {
        return (await musclesService.GetAllAsync(false, ct)).Select(m => new GuideItem
        {
            Category = GuideCategory.MuscleGroup,
            SourceId = m.Id,
            Title = m.Title,
            Description = m.Description,
            ImagePath = m.ImagePath
        }).ToList();
    }


}
