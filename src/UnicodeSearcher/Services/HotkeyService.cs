using System.Runtime.InteropServices;
using H.Hooks;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace UnicodeSearcher.Services;

/// <summary>
/// 글로벌 핫키 서비스 구현
/// </summary>
public class HotkeyService : IHotkeyService
{
    private readonly LowLevelKeyboardHook _hook;
    private WpfModifierKeys _modifiers = WpfModifierKeys.Control | WpfModifierKeys.Alt;
    private WpfKey _key = WpfKey.Space;
    private bool _isRegistered;
    private bool _disposed;

    // Win32 API로 키 상태 확인 (스레드 안전)
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VK_CONTROL = 0x11;
    private const int VK_MENU = 0x12;    // Alt
    private const int VK_SHIFT = 0x10;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;

    public event EventHandler? HotkeyPressed;

    public bool IsRegistered => _isRegistered;

    public HotkeyService()
    {
        _hook = new LowLevelKeyboardHook();
        _hook.Down += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyboardEventArgs e)
    {
        if (!_isRegistered) return;

        // 현재 눌린 모디파이어 키 확인 (Win32 API 사용)
        var currentModifiers = GetCurrentModifiers();

        // H.Hooks Key를 WPF Key로 변환
        var virtualKeyCode = (int)e.CurrentKey;
        var currentKey = System.Windows.Input.KeyInterop.KeyFromVirtualKey(virtualKeyCode);

        if (currentModifiers == _modifiers && currentKey == _key)
        {
            e.IsHandled = true;

            // UI 스레드에서 이벤트 발생
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    private static WpfModifierKeys GetCurrentModifiers()
    {
        var modifiers = WpfModifierKeys.None;

        // GetAsyncKeyState: 최상위 비트가 1이면 키가 눌린 상태
        if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
            modifiers |= WpfModifierKeys.Control;

        if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
            modifiers |= WpfModifierKeys.Alt;

        if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
            modifiers |= WpfModifierKeys.Shift;

        if ((GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0)
            modifiers |= WpfModifierKeys.Windows;

        return modifiers;
    }

    public bool RegisterHotkey(WpfModifierKeys modifiers, WpfKey key)
    {
        try
        {
            _modifiers = modifiers;
            _key = key;
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
        _isRegistered = false;
    }

    public void Start()
    {
        if (!_hook.IsStarted)
        {
            _hook.Start();
        }
    }

    public void Stop()
    {
        if (_hook.IsStarted)
        {
            _hook.Stop();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        UnregisterHotkey();
        _hook.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
