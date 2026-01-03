using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using JournalManagementSystem.Services;
using JournalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

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
			dbContext.Database.EnsureCreated();
		}

		// Apply saved theme immediately on startup
		var themeService = app.Services.GetRequiredService<IThemeService>();
		themeService.ApplyCurrentTheme();

		return app;
	}

}
