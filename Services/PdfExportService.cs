using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using JournalManagementSystem.Models;
using Markdig;
using PdfColors = QuestPDF.Helpers.Colors;

namespace JournalManagementSystem.Services;

public interface IPdfExportService
{
    Task<string> ExportJournalEntriesToPdfAsync(List<JournalEntry> entries, DateOnly? startDate = null, DateOnly? endDate = null);
}

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        // Set QuestPDF license for development/community use
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> ExportJournalEntriesToPdfAsync(List<JournalEntry> entries, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        return await Task.Run(() =>
        {
            // Generate filename with timestamp
            var fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // Create PDF document
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(PdfColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);

                    page.Content().Element(content => ComposeContent(content, entries, startDate, endDate));

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            })
            .GeneratePdf(filePath);

            return filePath;
        });
    }

    private void ComposeHeader(QuestPDF.Infrastructure.IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(PdfColors.Grey.Medium).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("📔 My Journal").FontSize(24).Bold().FontColor(PdfColors.Blue.Medium);
                    col.Item().Text($"Exported on {DateTime.Now:MMMM dd, yyyy}").FontSize(10).FontColor(PdfColors.Grey.Darken2);
                });
            });

            column.Item().PaddingTop(5).PaddingBottom(15);
        });
    }

    private void ComposeContent(QuestPDF.Infrastructure.IContainer container, List<JournalEntry> entries, DateOnly? startDate, DateOnly? endDate)
    {
        container.Column(column =>
        {
            // Export info
            column.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Export Summary").FontSize(14).Bold();
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        var dateRange = $"Date Range: {(startDate?.ToString("MMM dd, yyyy") ?? "Beginning")} - {(endDate?.ToString("MMM dd, yyyy") ?? "Present")}";
                        col.Item().Text(dateRange).FontSize(10).FontColor(PdfColors.Grey.Darken1);
                    }
                    col.Item().Text($"Total Entries: {entries.Count}").FontSize(10).FontColor(PdfColors.Grey.Darken1);
                });
            });

            // Entries
            foreach (var entry in entries.OrderByDescending(e => e.EntryDate))
            {
                column.Item().PaddingTop(15).Element(e => ComposeEntry(e, entry));
            }
        });
    }

    private void ComposeEntry(QuestPDF.Infrastructure.IContainer container, JournalEntry entry)
    {
        container.Column(column =>
        {
            // Entry header
            column.Item().Background(PdfColors.Grey.Lighten3).Padding(10).Column(headerCol =>
            {
                headerCol.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Title))
                        {
                            col.Item().Text(entry.Title).FontSize(16).Bold();
                        }
                        col.Item().Text(entry.EntryDate.ToString("MMMM dd, yyyy (dddd)")).FontSize(12).FontColor(PdfColors.Blue.Darken1);
                    });
                });

                // Moods
                headerCol.Item().PaddingTop(5).Row(row =>
                {
                    row.AutoItem().PaddingRight(10).Text($"{GetMoodEmoji(entry.PrimaryMood)} {entry.PrimaryMood}").FontSize(10).Bold().FontColor(PdfColors.Green.Darken2);

                    if (!string.IsNullOrWhiteSpace(entry.SecondaryMoodsCsv))
                    {
                        var secondaryMoods = entry.SecondaryMoodsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var mood in secondaryMoods)
                        {
                            var moodTrimmed = mood.Trim();
                            if (Enum.TryParse<Mood>(moodTrimmed, out var moodEnum))
                            {
                                row.AutoItem().PaddingRight(5).Text($"{GetMoodEmoji(moodEnum)} {moodTrimmed}").FontSize(9).FontColor(PdfColors.Grey.Darken1);
                            }
                        }
                    }
                });

                // Tags
                if (entry.JournalEntryTags?.Any() == true)
                {
                    headerCol.Item().PaddingTop(5).Row(row =>
                    {
                        row.AutoItem().PaddingRight(5).Text("Tags:").FontSize(9).FontColor(PdfColors.Grey.Darken2);
                        foreach (var tag in entry.JournalEntryTags)
                        {
                            row.AutoItem().PaddingRight(5).Background(PdfColors.Blue.Lighten3).Padding(3).Text(tag.Tag?.Name ?? "").FontSize(8);
                        }
                    });
                }
            });

            // Entry content
            column.Item().Padding(10).Column(contentCol =>
            {
                // Convert markdown to plain text for PDF
                var plainText = ConvertMarkdownToPlainText(entry.Content);
                contentCol.Item().Text(plainText).FontSize(11).LineHeight(1.5f);
            });

            // Entry footer
            column.Item().PaddingTop(5).BorderTop(1).BorderColor(PdfColors.Grey.Lighten2).PaddingTop(5)
                .Text($"Created: {entry.CreatedAt:MMM dd, yyyy HH:mm} | Updated: {entry.UpdatedAt:MMM dd, yyyy HH:mm}")
                .FontSize(8).FontColor(PdfColors.Grey.Darken1);

            // Separator
            column.Item().PaddingTop(10).BorderBottom(2).BorderColor(PdfColors.Grey.Lighten1);
        });
    }

    private string GetMoodEmoji(Mood mood) => mood switch
    {
        Mood.Happy => "😄",
        Mood.Excited => "🤩",
        Mood.Relaxed => "😌",
        Mood.Grateful => "🙏",
        Mood.Confident => "💪",
        Mood.Calm => "😌",
        Mood.Thoughtful => "🤔",
        Mood.Curious => "🧐",
        Mood.Nostalgic => "🕰️",
        Mood.Bored => "😐",
        Mood.Sad => "😢",
        Mood.Angry => "😠",
        Mood.Stressed => "😫",
        Mood.Lonely => "😔",
        Mood.Anxious => "😟",
        _ => "🙂"
    };

    private string ConvertMarkdownToPlainText(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        // Convert markdown to HTML first
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(markdown, pipeline);

        // Simple HTML tag removal for plain text
        var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

        // Decode HTML entities
        plainText = System.Net.WebUtility.HtmlDecode(plainText);

        return plainText;
    }
}
