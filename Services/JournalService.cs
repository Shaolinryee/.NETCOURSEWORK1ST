using Microsoft.EntityFrameworkCore;
using JournalManagementSystem.Data;
using JournalManagementSystem.Models;

namespace JournalManagementSystem.Services;

public interface IJournalService
{
    Task<JournalEntry?> GetEntryByDateAsync(DateOnly date);
    Task<List<JournalEntry>> GetEntriesAsync(int? limit = null);
    Task<List<JournalEntry>> GetEntriesRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<JournalEntry> CreateEntryAsync(string title, string content, int? moodRating = null);
    Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, int? moodRating = null);
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
            .Where(e => e.EntryDate == date)
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntry>> GetEntriesAsync(int? limit = null)
    {
        IQueryable<JournalEntry> query = _context.JournalEntries.OrderByDescending(e => e.EntryDate);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

   
    public async Task<List<JournalEntry>> GetEntriesRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.JournalEntries
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    
    public async Task<JournalEntry> CreateEntryAsync(string title, string content, int? moodRating = null)
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
            MoodRating = moodRating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    
    public async Task<JournalEntry> UpdateEntryAsync(int id, string title, string content, int? moodRating = null)
    {
        var entry = await _context.JournalEntries.FindAsync(id);
        if (entry == null)
        {
            throw new KeyNotFoundException($"Entry with ID {id} not found.");
        }

        entry.Title = title;
        entry.Content = content;
        entry.MoodRating = moodRating;
        entry.UpdatedAt = DateTime.UtcNow;

        _context.JournalEntries.Update(entry);
        await _context.SaveChangesAsync();

        return entry;
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
        return await _context.JournalEntries.FindAsync(id);
    }
}
