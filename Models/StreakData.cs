namespace JournalManagementSystem.Models;

/// <summary>
/// Represents streak statistics for journal entries
/// </summary>
public class StreakData
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly CurrentStreakStartDate { get; set; }
    public DateOnly CurrentStreakEndDate { get; set; }
    public DateOnly LongestStreakStartDate { get; set; }
    public DateOnly LongestStreakEndDate { get; set; }
    public int TotalEntriesThisMonth { get; set; }
    public int TotalEntriesAllTime { get; set; }
    public List<DateOnly> MissedDays { get; set; } = new();
    public DateTime LastEntryDate { get; set; }
    public int DaysSinceLastEntry { get; set; }
}
