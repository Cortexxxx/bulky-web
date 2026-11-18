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
    
    public static Task InvalidateCacheAsync(this IDatabaseAsync source, string key)
    {
        return ProceedRedisRequest(() => 
            source.StringDeleteAsync(key, ValueCondition.Always), false);
    }

    public static Task SetCacheAsync<T>(this IDatabaseAsync source, string key, T obj)
    {
        return ProceedRedisRequest(() => source.StringSetAsync(key,
            JsonSerializer.Serialize(obj, JsonSerializerOptions.Web),
            new Expiration(TimeSpan.FromSeconds(RedisConstants.DefaultCacheTtlSeconds))), false);
    }

    public static async Task<T?> GetCachedAsync<T>(this IDatabaseAsync source, string key) where T : class
    {
        var data = await ProceedRedisRequest(() => 
            source.StringGetAsync(key), RedisValue.Null);
        
        return data.HasValue 
            ? JsonSerializer.Deserialize<T>(data.ToString(), JsonSerializerOptions.Web)
            : null;
    }

    public static async Task InvalidateExercisesListAsync(this IDatabaseAsync source)
    {
        await source.InvalidateCacheAsync(RedisKeys.ExercisesList(true));
        await source.InvalidateCacheAsync(RedisKeys.ExercisesList(false));
    }

    public static async Task InvalidateExerciseAsync(this IDatabaseAsync source, Guid id)
    {
        await source.InvalidateCacheAsync(RedisKeys.ExerciseById(id));
        await source.InvalidateExercisesListAsync();
    }
    
    public static async Task InvalidateMuscleGroupAsync(this IDatabaseAsync source, Guid id)
    {
        await source.InvalidateCacheAsync(RedisKeys.MuscleGroupById(id));
        await source.InvalidateMuscleGroupsListAsync();
    }
    
    public static async Task InvalidateMuscleGroupsListAsync(this IDatabaseAsync source)
    {
        await source.InvalidateCacheAsync(RedisKeys.MuscleGroupsList(true));
        await source.InvalidateCacheAsync(RedisKeys.MuscleGroupsList(false));
    }

    public static async Task<int> GetVersion(this IDatabaseAsync source, string versionKey)
    {
        var version = await source.StringGetAsync(versionKey);
        
        if (!version.HasValue)
        {
            version = 0;
        }

        return (int)version;
    }
}