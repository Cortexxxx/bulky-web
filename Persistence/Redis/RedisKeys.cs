namespace Persistence.Redis;

internal static class RedisKeys
{
    public static string ExercisesList(bool includeArchived)
    {
        return $"bulky:exercises:v1:{(includeArchived ? "all" : "active")}";
    }

    public static string ExerciseById(Guid id)
    {
        return $"bulky:exercises:v1:{id}";
    }
}
