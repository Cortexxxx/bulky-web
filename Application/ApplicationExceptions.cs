namespace Application;
public class NoActiveSessionsException(Exception? inner = null) : UnauthorizedException("unauthorized", "No active sessions", inner: inner);
public class UnknownMuscleGroupException(Exception? inner = null) : BadRequestException("unknown_muscle_group", "Unknown muscle group", inner: inner);
public class WrongEmailOrPasswordException(Exception? inner = null) : UnauthorizedException("wrong_email_or_password", "Wrong email or password", inner: inner);
public class ConflictException(string error, string message = "Conflict", Exception? inner = null) : DomainException(error, message, inner: inner);
public class NotFoundException(string error, string message = "Not found", Exception? inner = null) : DomainException(error, message, inner: inner);
public class BadRequestException(string error, string message = "Bad request", Exception? inner = null) : DomainException(error, message, inner: inner);
public class UnauthorizedException(string error, string message = "Unauthorized", Exception? inner = null) : DomainException(error, message, inner: inner);

public abstract class DomainException(string error, string message, Exception? inner = null)
    : Exception(message: message, innerException: inner)
{
    public string Error { get; } = error;
}
