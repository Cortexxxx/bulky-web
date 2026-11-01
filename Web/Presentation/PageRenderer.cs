namespace Web.Presentation;

public class PageRenderer
{
    /// <summary>
    /// Read page file, render all partial parts of page
    /// </summary>
    /// <param name="relationPath">Path to page without www/pages/ prefix (ex. home.html)</param>
    public static async Task<string> RenderAsync(string relationPath)
    {
        var page = await File.ReadAllTextAsync($"www/pages/{relationPath}");
        page = await RenderHeaderAsync(page);
        return page;
    }
    
    private static async Task<string> RenderHeaderAsync(string page)
    {
        var header = await File.ReadAllTextAsync("www/pages/partials/header.html");
        return page.Replace("{{header}}", header);
    }
}