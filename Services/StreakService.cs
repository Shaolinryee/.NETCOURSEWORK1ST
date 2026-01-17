using JournalManagementSystem.Data;
using JournalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalManagementSystem.Services;

/// <summary>
/// Service for calculating and tracking journal entry streaks
/// </summary>
public interface IStreakService
{
    Task<StreakData> GetStreakDataAsync();
    Task<bool> HasEntryForDateAsync(DateOnly date);
    Task<int> GetCurrentStreakAsync();
    Task<int> GetLongestStreakAsync();
    Task<List<DateOnly>> GetMissedDaysThisMonthAsync();
    Task<int> GetTotalEntriesAsync();
    Task<int> GetEntriesThisMonthAsync();
}

public class StreakService : IStreakService
{
    private readonly AppDbContext _context;

    public StreakService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets comprehensive streak data
    /// </summary>
    public async Task<StreakData> GetStreakDataAsync()
    {
        var entries = await _context.JournalEntries
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        if (entries.Count == 0)
        {
            return new StreakData
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                TotalEntriesAllTime = 0,
                TotalEntriesThisMonth = 0,
                DaysSinceLastEntry = -1
            };
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var currentStreak = CalculateCurrentStreak(entries, today);
        var longestStreak = CalculateLongestStreak(entries);
        var missedDays = GetMissedDays(entries, today);
        var lastEntry = entries.Last();
        var daysSinceLastEntry = (today.ToDateTime(TimeOnly.MinValue) - lastEntry.EntryDate.ToDateTime(TimeOnly.MinValue)).Days;

        return new StreakData
        {
            CurrentStreak = currentStreak.Count,
            CurrentStreakStartDate = currentStreak.Start,
            CurrentStreakEndDate = currentStreak.End,
            LongestStreak = longestStreak.Count,
            LongestStreakStartDate = longestStreak.Start,
            LongestStreakEndDate = longestStreak.End,
            TotalEntriesAllTime = entries.Count,
            TotalEntriesThisMonth = GetEntriesInMonth(entries, today),
            MissedDays = missedDays,
            LastEntryDate = lastEntry.CreatedAt,
            DaysSinceLastEntry = daysSinceLastEntry
        };
    }

    /// <summary>
    /// Checks if there's an entry for a specific date
    /// </summary>
    public async Task<bool> HasEntryForDateAsync(DateOnly date)
    {
        return await _context.JournalEntries
            .AnyAsync(e => e.EntryDate == date);
    }

    /// <summary>
    /// Calculates the current streak
    /// </summary>
    public async Task<int> GetCurrentStreakAsync()
    {
        var entries = await _context.JournalEntries
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        if (entries.Count == 0) return 0;

        var today = DateOnly.FromDateTime(DateTime.Now);
        var streak = CalculateCurrentStreak(entries, today);
        return streak.Count;
    }

    /// <summary>
    /// Calculates the longest streak ever achieved
    /// </summary>
    public async Task<int> GetLongestStreakAsync()
    {
        var entries = await _context.JournalEntries
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        if (entries.Count == 0) return 0;

        var streak = CalculateLongestStreak(entries);
        return streak.Count;
    }

    /// <summary>
    /// Gets all missed days in the current month
    /// </summary>
    public async Task<List<DateOnly>> GetMissedDaysThisMonthAsync()
    {
        var entries = await _context.JournalEntries
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Now);
        return GetMissedDays(entries, today);
    }

    /// <summary>
    /// Gets total number of entries
    /// </summary>
    public async Task<int> GetTotalEntriesAsync()
    {
        return await _context.JournalEntries.CountAsync();
    }

    /// <summary>
    /// Gets entries in current month
    /// </summary>
    public async Task<int> GetEntriesThisMonthAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _context.JournalEntries
            .Where(e => e.EntryDate.Year == today.Year && e.EntryDate.Month == today.Month)
            .CountAsync();
    }

    /// <summary>
    /// Calculates current streak from today backwards
    /// </summary>
    private (int Count, DateOnly Start, DateOnly End) CalculateCurrentStreak(List<JournalEntry> entries, DateOnly today)
    {
        if (entries.Count == 0)
            return (0, today, today);

        var entrySet = entries.Select(e => e.EntryDate).ToHashSet();
        var streakCount = 0;
        var currentDate = today;
        DateOnly streakStart = today;

        // Check consecutive days backwards from today
        while (entrySet.Contains(currentDate))
        {
            streakCount++;
            streakStart = currentDate;
            currentDate = currentDate.AddDays(-1);
        }

        return (streakCount, streakStart, today);
    }

    /// <summary>
    /// Finds the longest streak in all entries
    /// </summary>
    private (int Count, DateOnly Start, DateOnly End) CalculateLongestStreak(List<JournalEntry> entries)
    {
        if (entries.Count == 0)
            return (0, DateOnly.MinValue, DateOnly.MinValue);

        var sortedEntries = entries.OrderBy(e => e.EntryDate).ToList();
        var longestStreakCount = 1;
        var longestStreakStart = sortedEntries[0].EntryDate;
        var longestStreakEnd = sortedEntries[0].EntryDate;

        var currentStreakCount = 1;
        var currentStreakStart = sortedEntries[0].EntryDate;

        for (int i = 1; i < sortedEntries.Count; i++)
        {
            var daysDifference = (sortedEntries[i].EntryDate.ToDateTime(TimeOnly.MinValue) -
                                sortedEntries[i - 1].EntryDate.ToDateTime(TimeOnly.MinValue)).Days;

            if (daysDifference == 1)
            {
                currentStreakCount++;
            }
            else
            {
                // Streak broken
                if (currentStreakCount > longestStreakCount)
                {
                    longestStreakCount = currentStreakCount;
                    longestStreakStart = currentStreakStart;
                    longestStreakEnd = sortedEntries[i - 1].EntryDate;
                }

                currentStreakCount = 1;
                currentStreakStart = sortedEntries[i].EntryDate;
            }
        }

        // Check final streak
        if (currentStreakCount > longestStreakCount)
        {
            longestStreakCount = currentStreakCount;
            longestStreakStart = currentStreakStart;
            longestStreakEnd = sortedEntries[^1].EntryDate;
        }

        return (longestStreakCount, longestStreakStart, longestStreakEnd);
    }

    /// <summary>
    /// Gets missed days in current month
    /// </summary>
    private List<DateOnly> GetMissedDays(List<JournalEntry> entries, DateOnly today)
    {
        var entrySet = entries.Select(e => e.EntryDate).ToHashSet();
        var missedDays = new List<DateOnly>();

        var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
        var currentDate = firstDayOfMonth;

        while (currentDate <= today)
        {
            if (!entrySet.Contains(currentDate) && currentDate <= today)
            {
                missedDays.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }

        return missedDays;
    }

    /// <summary>
    /// Gets entries in a specific month
    /// </summary>
    private int GetEntriesInMonth(List<JournalEntry> entries, DateOnly date)
    {
        return entries.Count(e =>
            e.EntryDate.Year == date.Year &&
            e.EntryDate.Month == date.Month);
    }
}
