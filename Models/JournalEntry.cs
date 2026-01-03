namespace JournalManagementSystem.Models;

public class JournalEntry
{
    public int Id { get; set; }


    public DateOnly EntryDate { get; set; }
    public string Content { get; set; } = string.Empty;

    public string? Title { get; set; }

    // Primary mood is required for analytics
    public Mood PrimaryMood { get; set; }

    // Up to two secondary moods, stored as comma-separated values (e.g. "Relaxed,Curious")
    public string? SecondaryMoodsCsv { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Tags association (many-to-many via join entity)
    public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();
}

public enum Mood
{
    // Positive
    Happy,
    Excited,
    Relaxed,
    Grateful,
    Confident,

    // Neutral
    Calm,
    Thoughtful,
    Curious,
    Nostalgic,
    Bored,

    // Negative
    Sad,
    Angry,
    Stressed,
    Lonely,
    Anxious
}

// Navigation for tags
// (Tags are exposed via the JournalEntryTags join entity)
