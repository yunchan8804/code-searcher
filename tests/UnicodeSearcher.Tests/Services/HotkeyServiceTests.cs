using UnicodeSearcher.Services;
using System.Windows.Input;

namespace UnicodeSearcher.Tests.Services;

/// <summary>
/// HotkeyService unit tests
/// - Win32 GetAsyncKeyState API integration (thread-safe)
/// - Hotkey registration/unregistration logic
/// </summary>
public class HotkeyServiceTests : IDisposable
{
    private readonly HotkeyService _hotkeyService;

    public HotkeyServiceTests()
    {
        _hotkeyService = new HotkeyService();
    }

    [Fact]
    public void Constructor_InitializesWithCorrectDefaults()
    {
        // Assert
        Assert.False(_hotkeyService.IsRegistered);
    }

    [Fact]
    public void RegisterHotkey_WithValidModifiersAndKey_ReturnsTrue()
    {
        // Arrange
        var modifiers = ModifierKeys.Control | ModifierKeys.Alt;
        var key = Key.Space;

        // Act
        var result = _hotkeyService.RegisterHotkey(modifiers, key);

        // Assert
        Assert.True(result);
        Assert.True(_hotkeyService.IsRegistered);
    }

    [Fact]
    public void RegisterHotkey_WithDifferentCombinations_ReturnsTrue()
    {
        // Test various modifier combinations
        var combinations = new[]
        {
            (ModifierKeys.Control, Key.C),
            (ModifierKeys.Alt, Key.F4),
            (ModifierKeys.Shift | ModifierKeys.Control, Key.S),
            (ModifierKeys.Windows, Key.E),
        };

        foreach (var (modifiers, key) in combinations)
        {
            // Act
            var result = _hotkeyService.RegisterHotkey(modifiers, key);

            // Assert
            Assert.True(result);
            Assert.True(_hotkeyService.IsRegistered);
        }
    }

    [Fact]
    public void UnregisterHotkey_AfterRegistration_SetsIsRegisteredToFalse()
    {
        // Arrange
        _hotkeyService.RegisterHotkey(ModifierKeys.Control | ModifierKeys.Alt, Key.Space);

        // Act
        _hotkeyService.UnregisterHotkey();

        // Assert
        Assert.False(_hotkeyService.IsRegistered);
    }

    [Fact]
    public void UnregisterHotkey_WithoutRegistration_DoesNotThrow()
    {
        // Act & Assert - should not throw
        var exception = Record.Exception(() => _hotkeyService.UnregisterHotkey());
        Assert.Null(exception);
    }

    [Fact]
    public void Start_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() => _hotkeyService.Start());
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_AfterStart_DoesNotThrow()
    {
        // Arrange
        _hotkeyService.Start();

        // Act & Assert
        var exception = Record.Exception(() => _hotkeyService.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void Stop_WithoutStart_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() => _hotkeyService.Stop());
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert - multiple dispose calls should be safe
        var exception = Record.Exception(() =>
        {
            _hotkeyService.Dispose();
            _hotkeyService.Dispose();
            _hotkeyService.Dispose();
        });

        Assert.Null(exception);
    }

    [Fact]
    public void RegisterHotkey_AfterUnregister_CanReRegister()
    {
        // Arrange
        _hotkeyService.RegisterHotkey(ModifierKeys.Control, Key.A);
        _hotkeyService.UnregisterHotkey();

        // Act
        var result = _hotkeyService.RegisterHotkey(ModifierKeys.Alt, Key.B);

        // Assert
        Assert.True(result);
        Assert.True(_hotkeyService.IsRegistered);
    }

    public void Dispose()
    {
        _hotkeyService.Dispose();
    }
}
