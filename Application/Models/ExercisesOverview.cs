namespace Application.Models;

public class ExercisesOverview
{
    public decimal LastWeekOrm { get; init; }
    public decimal LastWeekOrmIncrease { get; init; }
    
    public decimal LastMonthOrm { get; init; }
    public decimal LastMonthOrmIncrease { get; init; }
    
    public decimal MaxOrm { get; init; }
    public int MaxVolume { get; init; }
    public decimal MaxWeight { get; init; }
    
    public List<decimal> Orms = [];
    public List<int> Volumes = [];
}