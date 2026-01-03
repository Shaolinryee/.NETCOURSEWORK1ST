namespace JournalManagementSystem.Models;

public class JournalEntry
{
    public int Id { get; set; }

    
    public DateOnly EntryDate { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? Title { get; set; }

   
    public int? MoodRating { get; set; }

   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
