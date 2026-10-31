namespace Application.Exceptions;

public class EmailAlreadyExistsException(string message, Exception inner) : Exception(message, inner);