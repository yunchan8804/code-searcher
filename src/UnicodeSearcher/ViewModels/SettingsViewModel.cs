using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnicodeSearcher.Models;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.ViewModels;

/// <summary>
/// 설정 창 ViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly IStartupService _startupService;

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private bool _closeOnSelect = true;

    [ObservableProperty]
    private bool _clearSearchOnClose = true;

    [ObservableProperty]
    private bool _showInTaskbar = false;

    [ObservableProperty]
    private string _hotkeyDisplay = "Ctrl + Alt + Space";

    [ObservableProperty]
    private ModifierKeys _hotkeyModifiers = ModifierKeys.Control | ModifierKeys.Alt;

    [ObservableProperty]
    private Key _hotkeyKey = Key.Space;

    [ObservableProperty]
    private bool _runAtStartup = false;

    [ObservableProperty]
    private bool _startMinimized = true;

    /// <summary>
    /// 저장 완료 이벤트
    /// </summary>
    public event EventHandler? SaveCompleted;

    /// <summary>
    /// 취소 이벤트
    /// </summary>
    public event EventHandler? Cancelled;

    public SettingsViewModel(ISettingsService settingsService, IThemeService themeService, IStartupService startupService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _startupService = startupService;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.Settings;

        // 외관 설정
        SelectedTheme = settings.Appearance.Theme;

        // 동작 설정
        CloseOnSelect = settings.Behavior.CloseOnSelect;
        ClearSearchOnClose = settings.Behavior.ClearSearchOnClose;
        ShowInTaskbar = settings.Behavior.ShowInTaskbar;

        // 핫키 설정
        HotkeyModifiers = settings.Hotkey.ModifierKeys;
        HotkeyKey = settings.Hotkey.KeyValue;
        UpdateHotkeyDisplay();

        // 시작 설정
        RunAtStartup = settings.Startup.RunAtStartup;
        StartMinimized = settings.Startup.StartMinimized;
    }

    private void UpdateHotkeyDisplay()
    {
        var parts = new List<string>();

        if (HotkeyModifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (HotkeyModifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");
        if (HotkeyModifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");
        if (HotkeyModifiers.HasFlag(ModifierKeys.Windows))
            parts.Add("Win");

        parts.Add(HotkeyKey.ToString());

        HotkeyDisplay = string.Join(" + ", parts);
    }

    /// <summary>
    /// 핫키 변경
    /// </summary>
    public void SetHotkey(ModifierKeys modifiers, Key key)
    {
        HotkeyModifiers = modifiers;
        HotkeyKey = key;
        UpdateHotkeyDisplay();
    }

    /// <summary>
    /// 저장
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = _settingsService.Settings;

        // 외관 설정
        settings.Appearance.Theme = SelectedTheme;

        // 동작 설정
        settings.Behavior.CloseOnSelect = CloseOnSelect;
        settings.Behavior.ClearSearchOnClose = ClearSearchOnClose;
        settings.Behavior.ShowInTaskbar = ShowInTaskbar;

        // 핫키 설정
        settings.Hotkey.ModifierKeys = HotkeyModifiers;
        settings.Hotkey.KeyValue = HotkeyKey;

        // 시작 설정
        settings.Startup.RunAtStartup = RunAtStartup;
        settings.Startup.StartMinimized = StartMinimized;

        // 저장
        await _settingsService.SaveAsync();

        // 자동 시작 설정 적용
        if (RunAtStartup)
        {
            _startupService.Register(StartMinimized);
        }
        else
        {
            _startupService.Unregister();
        }

        // 테마 적용
        var themeMode = SelectedTheme switch
        {
            "Dark" => ThemeMode.Dark,
            "Light" => ThemeMode.Light,
            _ => ThemeMode.System
        };
        _themeService.SetTheme(themeMode);

        SaveCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 취소
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 기본값으로 초기화
    /// </summary>
    [RelayCommand]
    private void ResetToDefaults()
    {
        SelectedTheme = "System";
        CloseOnSelect = true;
        ClearSearchOnClose = true;
        ShowInTaskbar = false;
        HotkeyModifiers = ModifierKeys.Control | ModifierKeys.Alt;
        HotkeyKey = Key.Space;
        UpdateHotkeyDisplay();
        RunAtStartup = false;
        StartMinimized = true;
    }
}
