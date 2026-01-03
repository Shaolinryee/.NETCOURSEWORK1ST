using Microsoft.EntityFrameworkCore;
using JournalManagementSystem.Models;

namespace JournalManagementSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<JournalEntryTag> JournalEntryTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JournalEntry
        modelBuilder.Entity<JournalEntry>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(e => e.EntryDate)
            .IsUnique()
            .HasDatabaseName("IX_JournalEntry_EntryDate_Unique");

        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.Content)
            .IsRequired();

        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.EntryDate)
            .HasColumnType("DATE");

        // Store PrimaryMood as string and allow secondary moods CSV
        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.PrimaryMood)
            .HasConversion<string>()
            .IsRequired();

        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.SecondaryMoodsCsv)
            .HasColumnName("SecondaryMoods")
            .HasMaxLength(128);

        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<JournalEntry>()
            .Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure Tag and JournalEntryTag (explicit join entity)
        modelBuilder.Entity<Tag>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Name).IsRequired().HasMaxLength(64);
            b.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<JournalEntryTag>(jt =>
        {
            jt.HasKey(x => new { x.JournalEntryId, x.TagId });
            jt.HasOne(x => x.JournalEntry).WithMany(e => e.JournalEntryTags).HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            jt.HasOne(x => x.Tag).WithMany(t => t.JournalEntryTags).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
            jt.ToTable("JournalEntryTag");
        });
    }
}
