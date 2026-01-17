using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using JournalManagementSystem.Services;
using JournalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace JournalManagementSystem;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddBlazoredLocalStorage();
		builder.Services.AddScoped<IThemeService, ThemeService>();
		builder.Services.AddScoped<IMarkdownService, MarkdownService>();
		builder.Services.AddScoped<IStreakService, StreakService>();
		builder.Services.AddScoped<ISecurityService, SecurityService>();
		builder.Services.AddScoped<IPdfExportService, PdfExportService>();
		builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

		// Register DbContext
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Register Journal Service
		builder.Services.AddScoped<IJournalService, JournalService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Initialize database
		using (var scope = app.Services.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			// Ensure database exists
			dbContext.Database.EnsureCreated();

			// Initialize prebuilt tags if they don't exist
			try
			{
				var existingTags = dbContext.Tags.Where(t => t.IsPrebuilt).Count();
				if (existingTags == 0)
				{
					var prebuiltTags = new[]
					{
						new { Name = "Work", IsPrebuilt = true },
						new { Name = "Career", IsPrebuilt = true },
						new { Name = "Studies", IsPrebuilt = true },
						new { Name = "Health", IsPrebuilt = true },
						new { Name = "Fitness", IsPrebuilt = true },
						new { Name = "Relationships", IsPrebuilt = true },
						new { Name = "Family", IsPrebuilt = true },
						new { Name = "Friends", IsPrebuilt = true },
						new { Name = "Goals", IsPrebuilt = true },
						new { Name = "Achievements", IsPrebuilt = true },
						new { Name = "Challenges", IsPrebuilt = true },
						new { Name = "Gratitude", IsPrebuilt = true },
						new { Name = "Travel", IsPrebuilt = true },
						new { Name = "Hobbies", IsPrebuilt = true },
						new { Name = "Reflection", IsPrebuilt = true }
					};

					foreach (var tag in prebuiltTags)
					{
						dbContext.Tags.Add(new JournalManagementSystem.Models.Tag
						{
							Name = tag.Name,
							IsPrebuilt = tag.IsPrebuilt
						});
					}
					dbContext.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to initialize prebuilt tags: {ex.Message}");
			}

			// If the DB was created previously, its schema may not include newly added columns (PrimaryMood, SecondaryMoods).
			// Detect missing columns and ALTER TABLE to add them so runtime doesn't fail.
			try
			{
				var dbPathLocal = dbPath; // capture
				using var conn = new SqliteConnection($"Data Source={dbPathLocal}");
				conn.Open();

				var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = "PRAGMA table_info('JournalEntries');";
					using var reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						existingColumns.Add(reader.GetString(1)); // name column
					}
				}

				// Add PrimaryMood column if missing
				if (!existingColumns.Contains("PrimaryMood"))
				{
					using var addCmd = conn.CreateCommand();
					addCmd.CommandText = "ALTER TABLE JournalEntries ADD COLUMN PrimaryMood TEXT NOT NULL DEFAULT 'Calm';";
					addCmd.ExecuteNonQuery();
				}

				// Add SecondaryMoods column if missing
				if (!existingColumns.Contains("SecondaryMoods"))
				{
					using var addCmd2 = conn.CreateCommand();
					addCmd2.CommandText = "ALTER TABLE JournalEntries ADD COLUMN SecondaryMoods TEXT;";
					addCmd2.ExecuteNonQuery();
				}

				// Ensure join table for JournalEntryTag exists (created when tag support added)
				using (var checkJoin = conn.CreateCommand())
				{
					checkJoin.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='JournalEntryTag';";
					using var rdr = checkJoin.ExecuteReader();
					var hasJoin = rdr.Read();
					if (!hasJoin)
					{
						using var createJoin = conn.CreateCommand();
						createJoin.CommandText = @"CREATE TABLE IF NOT EXISTS JournalEntryTag (
							JournalEntryId INTEGER NOT NULL,
							TagId INTEGER NOT NULL,
							PRIMARY KEY (JournalEntryId, TagId),
							FOREIGN KEY (JournalEntryId) REFERENCES JournalEntries(Id) ON DELETE CASCADE,
							FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
						);";
						createJoin.ExecuteNonQuery();
					}
				}

				// Ensure Tags table exists
				using (var checkTags = conn.CreateCommand())
				{
					checkTags.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Tags';";
					using var rdrTags = checkTags.ExecuteReader();
					var hasTags = rdrTags.Read();
					if (!hasTags)
					{
						using var createTags = conn.CreateCommand();
						createTags.CommandText = @"CREATE TABLE IF NOT EXISTS Tags (
							Id INTEGER PRIMARY KEY AUTOINCREMENT,
							Name TEXT NOT NULL,
							IsPrebuilt INTEGER NOT NULL DEFAULT 0
						);";
						createTags.ExecuteNonQuery();
						using var createIdx = conn.CreateCommand();
						createIdx.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS IX_Tags_Name ON Tags(Name);";
						createIdx.ExecuteNonQuery();
					}
				}

				conn.Close();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"DB schema update check failed: {ex.Message}");
			}
		}

		// Apply saved theme immediately on startup
		var themeService = app.Services.GetRequiredService<IThemeService>();
		themeService.ApplyCurrentTheme();

		return app;
	}

}
