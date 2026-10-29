using System.Net;

namespace Bulky.Exceptions;

public class WebException(string message, HttpStatusCode statusCode, string error = "", Exception? innerException = null) 
    : Exception(message, innerException: innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Error { get; set; } = error;
}