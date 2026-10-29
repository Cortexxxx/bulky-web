using System.Diagnostics.CodeAnalysis;
using System.Net;
using WebException = Bulky.Exceptions.WebException;

namespace Bulky.Exceptions;

public static class ExceptionsHelpers
{
    [DoesNotReturn]
    public static void ThrowWebException(string message, HttpStatusCode code, string error)
    {
        throw new WebException(message, HttpStatusCode.NotFound, error);
    }

    [DoesNotReturn]
    public static void ThrowNotFound(string message = "Not found")
    {
        ThrowWebException(message, HttpStatusCode.NotFound, "not_found");
    }

    [DoesNotReturn]
    public static void ThrowBadRequest(string message = "Bad request", string error = "bad_request") => ThrowWebException(message, 
        HttpStatusCode.BadRequest, error);
}