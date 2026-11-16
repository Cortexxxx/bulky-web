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
}