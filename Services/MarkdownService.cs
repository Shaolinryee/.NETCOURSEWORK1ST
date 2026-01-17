using Markdig;

namespace JournalManagementSystem.Services;

/// <summary>
/// Service for converting Markdown text to HTML
/// Supports formatting like bold, italics, lists, headings, links, code blocks, and more
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// Converts markdown text to HTML
    /// </summary>
    string ConvertToHtml(string markdown);

    /// <summary>
    /// Gets a preview of markdown text (first 200 chars as plain text)
    /// </summary>
    string GetPreview(string markdown, int length = 200);
}

public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        // Create a markdown pipeline with extended features
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ConvertToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        try
        {
            return Markdown.ToHtml(markdown, _pipeline);
        }
        catch
        {
            // If markdown conversion fails, return the text wrapped in paragraph tags
            return $"<p>{System.Net.WebUtility.HtmlEncode(markdown)}</p>";
        }
    }

    public string GetPreview(string markdown, int length = 200)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        // Remove markdown syntax for preview
        var plainText = System.Text.RegularExpressions.Regex.Replace(markdown, @"[*_~`\[\]#]", string.Empty);

        if (plainText.Length > length)
            return plainText.Substring(0, length) + "...";

        return plainText;
    }
}
