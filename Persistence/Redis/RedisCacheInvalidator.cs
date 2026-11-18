using Application.Abstractions;
using StackExchange.Redis;

namespace Persistence.Redis;

public class RedisCacheInvalidator(IDatabaseAsync source) : IAnalyticsCacheInvalidator
{
    public async Task InvalidateAnalytics(Guid userId)
    {
        await source.StringIncrementAsync(RedisKeys.AnalyticsVersion(userId));
    }
}