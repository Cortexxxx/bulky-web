namespace Web;

/// <summary>
/// File system paths to frontend assets, relative to the working directory (build output).
/// </summary>
public static class FilePaths
{
    private const string Root = "www";

    public static readonly string Pages = Path.Combine(Root, "pages");
    public static readonly string ErrorPages = Path.Combine(Pages, "error-pages");
    public static readonly string Partials = Path.Combine(Pages, "partials");
    public static readonly string HeaderPartial = Path.Combine(Partials, "header.html");
    public static readonly string GuestsHeaderPartial = Path.Combine(Partials, "guests_header.html");
    public static readonly string FooterPartial = Path.Combine(Partials, "footer.html");
    public static readonly string Public = Path.Combine(Root, "public");
}
