using System.Text.Json;
using StackExchange.Redis;

namespace Persistence.Redis;

public static class RedisHelper
{
    public static async Task<T> ProceedRedisRequest<T>(Func<Task<T>> func, T defaultValue)
    {
        try
        {
            return await func();
        }
        catch (RedisConnectionException e)
        {
            Console.WriteLine($"Redis connection has problem: {e}");
        }
        catch (RedisTimeoutException e)
        {
            Console.WriteLine($"Redis timeout: {e}");
        }

        return defaultValue;
    }
    
    public static Task InvalidateCacheAsync(string key, IDatabaseAsync cacheDataSource)
    {
        return RedisHelper.ProceedRedisRequest(() => 
            cacheDataSource.StringDeleteAsync(key, ValueCondition.Always), false);
    }

    public static Task SetCacheAsync<T>(string key, T obj, IDatabaseAsync cacheDataSource)
    {
        return ProceedRedisRequest(() => cacheDataSource.StringSetAsync(key,
            JsonSerializer.Serialize(obj, JsonSerializerOptions.Web),
            new Expiration(TimeSpan.FromSeconds(RedisConstants.DefaultCacheTtlSeconds))), false);
    }

    public static T? GetCacheOrNull<T>(RedisValue data) where T : class
    {
        return data.HasValue
            ? JsonSerializer.Deserialize<T>(data.ToString(), JsonSerializerOptions.Web)
            : null;
    }
}