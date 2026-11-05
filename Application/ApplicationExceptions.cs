namespace Application;

public class EmailAlreadyExistsException(string message, Exception inner) : Exception(message, inner);

public class ExerciseExistsException(string message, Exception inner) : Exception(message: message, innerException: inner);

public class MuscleGroupExistsException(string message, Exception inner) : Exception(message: message, innerException: inner);

public class NoActiveSessionsException : Exception;

public class UnknownMuscleGroupException(string message, Exception inner) : Exception(message: message, innerException: inner);

public class WrongEmailOrPasswordException(string message) : Exception(message);
