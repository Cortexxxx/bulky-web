namespace Persistence.Redis;

internal static class RedisKeys
{
    public static string ExercisesList(bool includeArchived) =>
        $"bulky:exercises:v1:{(includeArchived ? "all" : "active")}";
    
    public static string ExerciseById(Guid id) => $"bulky:exercises:v1:{id}";
    
    public static string MuscleGroupsList(bool includeArchived) => 
        $"bulky:muscleGroups:v1:{(includeArchived ? "all" : "active")}";
    
    public static string MuscleGroupById(Guid id) => $"bulky:muscleGroups:v1:{id}";
    
    public static string WorkoutOverview(int version, Guid userId) => 
        $"bulky:analytics:workouts:v1:{userId}:{version}";
    
    public static string ExerciseOverview(int version, Guid userId, Guid exerciseId) => 
        $"bulky:analytics:exercises:v1:{userId}:{version}:{exerciseId}";
    
    public static string MuscleGroupOverview(int version, Guid userId, Guid muscleGroupId) => 
        $"bulky:analytics:muscleGroups:v1:{userId}:{version}:{muscleGroupId}";

    public static string AnalyticsVersion(Guid userId) => $"bulky:analytics:version:{userId}";
}
