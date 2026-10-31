using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Bulky.Exceptions;

public static class ExceptionsHelpers
{
    [DoesNotReturn]
    public static void ThrowWebException(string message, HttpStatusCode code, string error)
    {
        throw new WebException(message, code, error);
    }

    [DoesNotReturn]
    public static void ThrowNotFound(string message = "Not found")
    {
        ThrowWebException(message, HttpStatusCode.NotFound, "not_found");
    }

    [DoesNotReturn]
    public static void ThrowBadRequest(string message = "Bad request", string error = "bad_request") => ThrowWebException(message, 
        HttpStatusCode.BadRequest, error);
    
    [DoesNotReturn]
    public static void ThrowUnauthorized(string message = "Unauthorized")
    {
        ThrowWebException(message, HttpStatusCode.Unauthorized, "unauthorized");
    }
}