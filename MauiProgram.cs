using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using JournalManagementSystem.Services;

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

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

    // Apply saved theme immediately on startup
    var themeService = app.Services.GetRequiredService<IThemeService>();
    themeService.ApplyCurrentTheme();

    return app;
	}
	
}
