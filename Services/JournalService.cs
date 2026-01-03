using Microsoft.EntityFrameworkCore;
using JournalManagementSystem.Data;
using JournalManagementSystem.Models;

namespace JournalManagementSystem.Services;

public interface IJournalService
{
    Task<JournalEntry?> GetEntryByDateAsync(DateOnly date);
    Task<List<JournalEntry>> GetEntriesAsync(int? limit = null);
    Task<List<JournalEntry>> GetEntriesRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<JournalEntry> CreateEntryAsync(string title, string content, Mood primaryMood, List<Mood>? secondaryMoods = null);
    Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, Mood primaryMood, List<Mood>? secondaryMoods = null);
    Task<JournalEntry> CreateEntryAsync(string title, string content, Mood primaryMood, List<Mood>? secondaryMoods, List<string>? tags);
    Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, Mood primaryMood, List<Mood>? secondaryMoods, List<string>? tags);
    Task<List<Tag>> GetAllTagsAsync();
    Task<bool> DeleteEntryAsync(int id);
    Task<bool> EntryExistsForDateAsync(DateOnly date);
    Task<JournalEntry?> GetEntryByIdAsync(int id);
}

public class JournalService : IJournalService
{
    private readonly AppDbContext _context;

    public JournalService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<JournalEntry?> GetEntryByDateAsync(DateOnly date)
    {
        return await _context.JournalEntries
            .Include(e => e.JournalEntryTags).ThenInclude(jt => jt.Tag)
            .Where(e => e.EntryDate == date)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesAsync(int? limit = null)
    {
        IQueryable<JournalEntry> query = _context.JournalEntries
            .Include(e => e.JournalEntryTags).ThenInclude(jt => jt.Tag)
            .OrderByDescending(e => e.EntryDate);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }


    public async Task<List<JournalEntry>> GetEntriesRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.JournalEntries
            .Include(e => e.JournalEntryTags).ThenInclude(jt => jt.Tag)
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }


    public async Task<JournalEntry> CreateEntryAsync(string title, string content, Mood primaryMood, List<Mood>? secondaryMoods = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);


        var existingEntry = await GetEntryByDateAsync(today);
        if (existingEntry != null)
        {
            throw new InvalidOperationException($"An entry already exists for {today}. Please update the existing entry or delete it first.");
        }

        var entry = new JournalEntry
        {
            EntryDate = today,
            Title = title,
            Content = content,
            PrimaryMood = primaryMood,
            SecondaryMoodsCsv = (secondaryMoods == null || secondaryMoods.Count == 0) ? null : string.Join(',', secondaryMoods.Select(m => m.ToString())),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    // New overloaded CreateEntry with tags
    public async Task<JournalEntry> CreateEntryAsync(string title, string content, Mood primaryMood, List<Mood>? secondaryMoods, List<string>? tags)
    {
        var entry = await CreateEntryAsync(title, content, primaryMood, secondaryMoods);

        if (tags != null && tags.Count > 0)
        {
            await EnsureAndAttachTagsAsync(entry, tags);
            await _context.SaveChangesAsync();
        }

        return entry;
    }


    public async Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, Mood primaryMood, List<Mood>? secondaryMoods = null)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null)
        {
            throw new KeyNotFoundException($"Entry with ID {id} not found.");
        }

        entry.Title = title;
        entry.Content = content;
        entry.PrimaryMood = primaryMood;
        entry.SecondaryMoodsCsv = (secondaryMoods == null || secondaryMoods.Count == 0) ? null : string.Join(',', secondaryMoods.Select(m => m.ToString()));
        entry.UpdatedAt = DateTime.UtcNow;

        _context.JournalEntries.Update(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    // New overloaded Update with tags
    public async Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, Mood primaryMood, List<Mood>? secondaryMoods, List<string>? tags)
    {
        var entry = await UpdateEntryAsync(id, title, content, primaryMood, secondaryMoods);

        // Handle tags
        await EnsureAndAttachTagsAsync(entry, tags ?? new List<string>());
        await _context.SaveChangesAsync();

        return entry;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.OrderBy(t => t.Name).ToListAsync();
    }



    private async Task EnsureAndAttachTagsAsync(JournalEntry entry, List<string> tags)
    {
        if (tags == null || tags.Count == 0) return;

        var normalized = tags.Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existing = await _context.Tags.Where(t => normalized.Contains(t.Name)).ToListAsync();

        var tagEntities = new List<Tag>();
        foreach (var tagName in normalized)
        {
            var existingTag = existing.FirstOrDefault(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase));
            if (existingTag != null)
            {
                tagEntities.Add(existingTag);
            }
            else
            {
                var newTag = new Tag { Name = tagName };
                _context.Tags.Add(newTag);
                tagEntities.Add(newTag);
            }
        }

        // Attach JournalEntryTag relations (replace existing ones)
        entry.JournalEntryTags.Clear();
        foreach (var t in tagEntities)
        {
            entry.JournalEntryTags.Add(new JournalEntryTag { JournalEntry = entry, Tag = t });
        }
    }


    public async Task<bool> DeleteEntryAsync(int id)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null)
        {
            return false;
        }

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EntryExistsForDateAsync(DateOnly date)
    {
        return await _context.JournalEntries
            .AnyAsync(e => e.EntryDate == date);
    }


    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _context.JournalEntries
            .Include(e => e.JournalEntryTags).ThenInclude(jt => jt.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
