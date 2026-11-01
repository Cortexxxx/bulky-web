namespace Application.Exceptions;

public class MuscleGroupExistsException(string message, Exception inner) : Exception(message: message, innerException: inner);