using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace JournalManagementSystem.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    Task ToggleTheme();
    Task InitializeAsync();
    void ApplyCurrentTheme(); // Called from MAUI side
}

public class ThemeService : IThemeService
{
    private const string ThemeKey = "appTheme";
    private readonly ILocalStorageService _localStorage;

    public bool IsDarkMode { get; private set; }

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        IsDarkMode = await _localStorage.GetItemAsync<bool?>(ThemeKey) ?? false;
        ApplyCurrentTheme();
    }

    public async Task ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        await _localStorage.SetItemAsync(ThemeKey, IsDarkMode);
        ApplyCurrentTheme();
    }

    public void ApplyCurrentTheme()
    {
        Application.Current!.Resources.TryGetValue("PageBackgroundColor", out var lightBg);
        Application.Current!.Resources.TryGetValue("PageBackgroundColorDark", out var darkBg);
        Application.Current!.Resources.TryGetValue("TextColor", out var lightText);
        Application.Current!.Resources.TryGetValue("TextColorDark", out var darkText);
        // Add more as needed

        Application.Current.Resources["PageBackgroundColor"] = IsDarkMode ? darkBg : lightBg;
        Application.Current.Resources["TextColor"] = IsDarkMode ? darkText : lightText;
        Application.Current.Resources["PrimaryColor"] = IsDarkMode ? Application.Current.Resources["PrimaryColorDark"] : Application.Current.Resources["PrimaryColor"];
        Application.Current.Resources["CardBackgroundColor"] = IsDarkMode ? Application.Current.Resources["CardBackgroundColorDark"] : Application.Current.Resources["CardBackgroundColor"];
    }
}