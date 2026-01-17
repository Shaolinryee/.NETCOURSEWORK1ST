using JournalManagementSystem.Data;
using JournalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalManagementSystem.Services;

public interface IAnalyticsService
{
    Task<DashboardAnalytics> GetDashboardAnalyticsAsync();
    Task<List<MoodDistribution>> GetMoodDistributionAsync();
    Task<List<TagUsage>> GetTopTagsAsync(int limit = 10);
    Task<List<WordCountTrend>> GetWordCountTrendsAsync(int days = 30);
    Task<MoodStatistics> GetMoodStatisticsAsync();
}

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync()
    {
        var entries = await _context.JournalEntries
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jt => jt.Tag)
            .ToListAsync();

        var totalEntries = entries.Count;
        var totalWords = entries.Sum(e => CountWords(e.Content));
        var avgWordsPerEntry = totalEntries > 0 ? totalWords / totalEntries : 0;

        var moodDistribution = await GetMoodDistributionAsync();
        var topTags = await GetTopTagsAsync(5);
        var moodStats = await GetMoodStatisticsAsync();

        return new DashboardAnalytics
        {
            TotalEntries = totalEntries,
            TotalWords = totalWords,
            AverageWordsPerEntry = avgWordsPerEntry,
            MoodDistribution = moodDistribution,
            TopTags = topTags,
            MoodStatistics = moodStats
        };
    }

    public async Task<List<MoodDistribution>> GetMoodDistributionAsync()
    {
        var entries = await _context.JournalEntries.ToListAsync();

        var moodCounts = new Dictionary<Mood, int>();

        // Count primary moods
        foreach (var entry in entries)
        {
            if (!moodCounts.ContainsKey(entry.PrimaryMood))
                moodCounts[entry.PrimaryMood] = 0;
            moodCounts[entry.PrimaryMood]++;

            // Count secondary moods
            if (!string.IsNullOrWhiteSpace(entry.SecondaryMoodsCsv))
            {
                var secondaryMoods = entry.SecondaryMoodsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var moodStr in secondaryMoods)
                {
                    if (Enum.TryParse<Mood>(moodStr.Trim(), out var mood))
                    {
                        if (!moodCounts.ContainsKey(mood))
                            moodCounts[mood] = 0;
                        moodCounts[mood]++;
                    }
                }
            }
        }

        var total = moodCounts.Values.Sum();

        return moodCounts
            .Select(m => new MoodDistribution
            {
                Mood = m.Key,
                Count = m.Value,
                Percentage = total > 0 ? (double)m.Value / total * 100 : 0
            })
            .OrderByDescending(m => m.Count)
            .ToList();
    }

    public async Task<List<TagUsage>> GetTopTagsAsync(int limit = 10)
    {
        var tagStats = await _context.JournalEntryTags
            .GroupBy(jt => jt.Tag)
            .Select(g => new TagUsage
            {
                TagName = g.Key.Name,
                UsageCount = g.Count()
            })
            .OrderByDescending(t => t.UsageCount)
            .Take(limit)
            .ToListAsync();

        return tagStats;
    }

    public async Task<List<WordCountTrend>> GetWordCountTrendsAsync(int days = 30)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));
        var entries = await _context.JournalEntries
            .Where(e => e.EntryDate >= startDate)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();

        return entries
            .Select(e => new WordCountTrend
            {
                Date = e.EntryDate,
                WordCount = CountWords(e.Content)
            })
            .ToList();
    }

    public async Task<MoodStatistics> GetMoodStatisticsAsync()
    {
        var entries = await _context.JournalEntries.ToListAsync();

        if (!entries.Any())
        {
            return new MoodStatistics();
        }

        // Get mood frequencies
        var moodCounts = new Dictionary<Mood, int>();
        foreach (var entry in entries)
        {
            if (!moodCounts.ContainsKey(entry.PrimaryMood))
                moodCounts[entry.PrimaryMood] = 0;
            moodCounts[entry.PrimaryMood]++;
        }

        var mostFrequent = moodCounts.OrderByDescending(m => m.Value).FirstOrDefault();
        var leastFrequent = moodCounts.OrderBy(m => m.Value).FirstOrDefault();

        // Get recent mood trend (last 7 days)
        var recentEntries = entries
            .Where(e => e.EntryDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)))
            .OrderByDescending(e => e.EntryDate)
            .Take(5)
            .ToList();

        return new MoodStatistics
        {
            MostFrequentMood = mostFrequent.Key,
            MostFrequentCount = mostFrequent.Value,
            LeastFrequentMood = leastFrequent.Key,
            LeastFrequentCount = leastFrequent.Value,
            TotalMoodEntries = entries.Count,
            RecentMoods = recentEntries.Select(e => e.PrimaryMood).ToList()
        };
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

// Analytics Models
public class DashboardAnalytics
{
    public int TotalEntries { get; set; }
    public int TotalWords { get; set; }
    public int AverageWordsPerEntry { get; set; }
    public List<MoodDistribution> MoodDistribution { get; set; } = new();
    public List<TagUsage> TopTags { get; set; } = new();
    public MoodStatistics MoodStatistics { get; set; } = new();
}

public class MoodDistribution
{
    public Mood Mood { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TagUsage
{
    public string TagName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class WordCountTrend
{
    public DateOnly Date { get; set; }
    public int WordCount { get; set; }
}

public class MoodStatistics
{
    public Mood MostFrequentMood { get; set; }
    public int MostFrequentCount { get; set; }
    public Mood LeastFrequentMood { get; set; }
    public int LeastFrequentCount { get; set; }
    public int TotalMoodEntries { get; set; }
    public List<Mood> RecentMoods { get; set; } = new();
}
