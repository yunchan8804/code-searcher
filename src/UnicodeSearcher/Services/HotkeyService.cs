using NHotkey;
using NHotkey.Wpf;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace UnicodeSearcher.Services;

/// <summary>
/// 글로벌 핫키 서비스 구현 (NHotkey 사용)
/// RegisterHotKey Win32 API를 사용하여 키 입력을 완전히 소비함
/// </summary>
public class HotkeyService : IHotkeyService
{
    private const string HotkeyName = "UnicodeSearcher_GlobalHotkey";

    private WpfModifierKeys _modifiers = WpfModifierKeys.Control | WpfModifierKeys.Alt;
    private WpfKey _key = WpfKey.Space;
    private bool _isRegistered;
    private bool _isStarted;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;

    public bool IsRegistered => _isRegistered;

    public HotkeyService()
    {
    }

    private void OnHotkeyPressed(object? sender, HotkeyEventArgs e)
    {
        e.Handled = true;
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public bool RegisterHotkey(WpfModifierKeys modifiers, WpfKey key)
    {
        try
        {
            // 기존 핫키 제거
            if (_isRegistered)
            {
                UnregisterHotkey();
            }

            _modifiers = modifiers;
            _key = key;

            // 이미 시작된 상태면 바로 등록
            if (_isStarted)
            {
                HotkeyManager.Current.AddOrReplace(HotkeyName, _key, _modifiers, OnHotkeyPressed);
            }

            _isRegistered = true;
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to register hotkey: {ex.Message}");
            return false;
        }
    }

    public void UnregisterHotkey()
    {
        try
        {
            HotkeyManager.Current.Remove(HotkeyName);
        }
        catch
        {
            // 핫키가 등록되지 않은 경우 무시
        }
        _isRegistered = false;
    }

    public void Start()
    {
        if (_isStarted) return;

        _isStarted = true;

        // 등록된 핫키가 있으면 활성화
        if (_isRegistered)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(HotkeyName, _key, _modifiers, OnHotkeyPressed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start hotkey: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        if (!_isStarted) return;

        _isStarted = false;

        try
        {
            HotkeyManager.Current.Remove(HotkeyName);
        }
        catch
        {
            // 무시
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        UnregisterHotkey();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
