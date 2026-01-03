using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace JournalManagementSystem.Services;

public enum AppTheme
{
    Light,
    Dark
}

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    Task<AppTheme> GetThemeAsync();
    Task SetThemeAsync(AppTheme theme);
    Task InitializeAsync();
    void ApplyCurrentTheme();
    List<AppTheme> GetAvailableThemes();
}

public class ThemeService : IThemeService
{
    private const string ThemeKey = "appTheme";
    private readonly ILocalStorageService _localStorage;
    private AppTheme _currentTheme = AppTheme.Light;

    public AppTheme CurrentTheme => _currentTheme;

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        var savedTheme = await _localStorage.GetItemAsync<string>(ThemeKey);
        if (!string.IsNullOrEmpty(savedTheme) && Enum.TryParse<AppTheme>(savedTheme, out var theme))
        {
            _currentTheme = theme;
        }
        ApplyCurrentTheme();
    }

    public async Task<AppTheme> GetThemeAsync()
    {
        return await Task.FromResult(_currentTheme);
    }

    public async Task SetThemeAsync(AppTheme theme)
    {
        _currentTheme = theme;
        await _localStorage.SetItemAsync(ThemeKey, theme.ToString());
        ApplyCurrentTheme();
    }

    public List<AppTheme> GetAvailableThemes()
    {
        return Enum.GetValues(typeof(AppTheme)).Cast<AppTheme>().ToList();
    }

    public void ApplyCurrentTheme()
    {
        try
        {
            if (Application.Current == null) return;

            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
        }
    }
}