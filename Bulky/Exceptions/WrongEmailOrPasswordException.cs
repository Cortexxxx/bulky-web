namespace Bulky.Exceptions;

public class WrongEmailOrPasswordException(string message) : Exception(message);