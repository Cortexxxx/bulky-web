namespace Application.Exceptions;

public class WrongEmailOrPasswordException(string message) : Exception(message);