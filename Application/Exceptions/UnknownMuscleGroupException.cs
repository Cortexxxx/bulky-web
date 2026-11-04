namespace Application.Exceptions;

public class UnknownMuscleGroupException(string message, Exception inner) : Exception(message: message, innerException: inner);