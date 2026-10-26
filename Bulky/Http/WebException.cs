using System.Net;

namespace Bulky;

public class WebException(string message, HttpStatusCode statusCode, string error = "") : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Error { get; set; } = error;
}