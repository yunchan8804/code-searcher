using System.Windows;
using Microsoft.Win32;

namespace UnicodeSearcher.Services;

/// <summary>
/// 테마 서비스 구현
/// </summary>
public class ThemeService : IThemeService
{
    private ThemeMode _currentMode = ThemeMode.Light;

    public ThemeMode CurrentMode => _currentMode;

    public ThemeMode ActualTheme => _currentMode == ThemeMode.System
        ? GetSystemTheme()
        : _currentMode;

    public event EventHandler<ThemeMode>? ThemeChanged;

    public ThemeService()
    {
        // 시스템 테마 변경 감지 (선택적)
        SystemEvents.UserPreferenceChanged += OnSystemPreferenceChanged;
    }

    private void OnSystemPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General && _currentMode == ThemeMode.System)
        {
            ApplyTheme(ActualTheme);
            ThemeChanged?.Invoke(this, ActualTheme);
        }
    }

    public void SetTheme(ThemeMode mode)
    {
        _currentMode = mode;
        ApplyTheme(ActualTheme);
        ThemeChanged?.Invoke(this, ActualTheme);
    }

    private void ApplyTheme(ThemeMode theme)
    {
        var app = Application.Current;
        if (app == null) return;

        var resources = app.Resources.MergedDictionaries;

        // 기존 테마 제거
        var oldTheme = resources.FirstOrDefault(r =>
            r.Source?.OriginalString.Contains("Theme.xaml") == true);
        if (oldTheme != null)
        {
            resources.Remove(oldTheme);
        }

        // 새 테마 적용
        var themeUri = theme == ThemeMode.Dark
            ? "Resources/Themes/DarkTheme.xaml"
            : "Resources/Themes/LightTheme.xaml";

        try
        {
            resources.Add(new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
        }
    }

    private static ThemeMode GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            var value = key?.GetValue("AppsUseLightTheme");
            return value is int v && v == 0 ? ThemeMode.Dark : ThemeMode.Light;
        }
        catch
        {
            return ThemeMode.Light;
        }
    }
}
