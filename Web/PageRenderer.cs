namespace Web;

public static class PageRenderer
{
    private const string HeaderPlaceholder = "{{header}}";
    private const string GuestsHeaderPlaceholder = "{{guests_header}}";
    private const string FooterPlaceholder = "{{footer}}";
    private const string ToastsPlaceholder = "{{toasts}}";

    public static async Task<string> RenderAsync(string relationPath)
    {
        return await RenderFileAsync(Path.Combine(FilePaths.Pages, relationPath));
    }
    
    public static async Task<string> RenderFileAsync(string path)
    {
        var page = await File.ReadAllTextAsync(path);
        page = await RenderPartialAsync(page, HeaderPlaceholder, FilePaths.HeaderPartial);
        page = await RenderPartialAsync(page, GuestsHeaderPlaceholder, FilePaths.GuestsHeaderPartial);
        page = await RenderPartialAsync(page, FooterPlaceholder, FilePaths.FooterPartial);
        page = await RenderPartialAsync(page, ToastsPlaceholder, FilePaths.ToastsPartial);        return page;
    }

    private static async Task<string> RenderPartialAsync(string page, string placeholder, string partialPath)
    {
        if (!page.Contains(placeholder)) return page;

        var partial = await File.ReadAllTextAsync(partialPath);
        return page.Replace(placeholder, partial);
    }
}
