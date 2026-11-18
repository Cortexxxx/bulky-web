namespace Application.Abstractions;

public interface IAnalyticsCacheInvalidator
{
    public Task InvalidateAnalytics(Guid userId);
}