using System.Text;

namespace Bulky.Http;

public record CookieOptions
{
    public bool HttpOnly { get; init; } = true;
    public bool Secure { get; init; } = false;
    public SameSite SameSite { get; init; } = SameSite.None;
    public string Path { get; init; } = "/";
    public TimeSpan? MaxAge;

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(HttpOnly ? "HttpOnly; " : "");
        stringBuilder.Append(Secure ? "Secure; " : "");
        stringBuilder.Append("Path=" + Path + "; ");
        stringBuilder.Append("SameSite=" + SameSite + "; ");
        
        if (MaxAge != null)
        {
            stringBuilder.Append("Max-Age=" + (long)MaxAge.Value.TotalSeconds + "; ");
        }

        return stringBuilder.ToString();
    }
}