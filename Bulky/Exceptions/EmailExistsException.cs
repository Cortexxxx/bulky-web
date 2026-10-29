namespace Bulky.Exceptions;

public class EmailAlreadyExistsException(string message, Exception inner) : Exception(message, inner);