namespace Application.Exceptions;

public class ExerciseExistsException(string message, Exception inner) : Exception(message: message, innerException: inner);