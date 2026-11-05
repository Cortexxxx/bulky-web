namespace Web;

public static class PageRenderer
{
    private const string HeaderPlaceholder = "{{header}}";
    private const string GuestsHeaderPlaceholder = "{{guests_header}}";
    private const string FooterPlaceholder = "{{footer}}";

    /// <summary>
    /// Read page file, render all partial parts of page
    /// </summary>
    /// <param name="relationPath">Path to page without www/pages/ prefix (ex. home.html)</param>
    public static async Task<string> RenderAsync(string relationPath)
    {
        return await RenderFileAsync(Path.Combine(FilePaths.Pages, relationPath));
    }

    /// <summary>
    /// Read page by its full path (ex. error pages), render all partial parts of page
    /// </summary>
    public static async Task<string> RenderFileAsync(string path)
    {
        var page = await File.ReadAllTextAsync(path);
        page = await RenderPartialAsync(page, HeaderPlaceholder, FilePaths.HeaderPartial);
        page = await RenderPartialAsync(page, GuestsHeaderPlaceholder, FilePaths.GuestsHeaderPartial);
        page = await RenderPartialAsync(page, FooterPlaceholder, FilePaths.FooterPartial);
        return page;
    }

    private static async Task<string> RenderPartialAsync(string page, string placeholder, string partialPath)
    {
        if (!page.Contains(placeholder)) return page;

        var partial = await File.ReadAllTextAsync(partialPath);
        return page.Replace(placeholder, partial);
    }
}
