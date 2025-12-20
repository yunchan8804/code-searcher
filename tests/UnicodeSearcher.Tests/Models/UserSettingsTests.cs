using System.Windows.Input;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Tests.Models;

public class UserSettingsTests
{
    [Fact]
    public void HotkeySettings_DefaultValues()
    {
        // Arrange & Act
        var settings = new HotkeySettings();

        // Assert
        Assert.Equal("Ctrl+Alt", settings.Modifiers);
        Assert.Equal("Space", settings.Key);
        Assert.Equal(ModifierKeys.Control | ModifierKeys.Alt, settings.ModifierKeys);
        Assert.Equal(Key.Space, settings.KeyValue);
    }

    [Theory]
    [InlineData("Ctrl", ModifierKeys.Control)]
    [InlineData("Alt", ModifierKeys.Alt)]
    [InlineData("Shift", ModifierKeys.Shift)]
    [InlineData("Win", ModifierKeys.Windows)]
    [InlineData("Ctrl+Alt", ModifierKeys.Control | ModifierKeys.Alt)]
    [InlineData("Ctrl+Shift", ModifierKeys.Control | ModifierKeys.Shift)]
    [InlineData("Ctrl+Alt+Shift", ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)]
    public void HotkeySettings_ModifierKeys_ParsesCorrectly(string modifiers, ModifierKeys expected)
    {
        // Arrange
        var settings = new HotkeySettings { Modifiers = modifiers };

        // Act & Assert
        Assert.Equal(expected, settings.ModifierKeys);
    }

    [Fact]
    public void HotkeySettings_ModifierKeys_Setter_UpdatesModifiers()
    {
        // Arrange
        var settings = new HotkeySettings();

        // Act
        settings.ModifierKeys = ModifierKeys.Control | ModifierKeys.Shift;

        // Assert
        Assert.Contains("Ctrl", settings.Modifiers);
        Assert.Contains("Shift", settings.Modifiers);
        Assert.DoesNotContain("Alt", settings.Modifiers);
    }

    [Theory]
    [InlineData("Space", Key.Space)]
    [InlineData("A", Key.A)]
    [InlineData("F1", Key.F1)]
    [InlineData("Enter", Key.Enter)]
    public void HotkeySettings_KeyValue_ParsesCorrectly(string key, Key expected)
    {
        // Arrange
        var settings = new HotkeySettings { Key = key };

        // Act & Assert
        Assert.Equal(expected, settings.KeyValue);
    }

    [Fact]
    public void HotkeySettings_KeyValue_Setter_UpdatesKey()
    {
        // Arrange
        var settings = new HotkeySettings();

        // Act
        settings.KeyValue = Key.F12;

        // Assert
        Assert.Equal("F12", settings.Key);
    }

    [Fact]
    public void HotkeySettings_InvalidKey_DefaultsToSpace()
    {
        // Arrange
        var settings = new HotkeySettings { Key = "InvalidKey" };

        // Act & Assert
        Assert.Equal(Key.Space, settings.KeyValue);
    }

    [Fact]
    public void BehaviorSettings_DefaultValues()
    {
        // Arrange & Act
        var settings = new BehaviorSettings();

        // Assert
        Assert.True(settings.CloseOnSelect);
        Assert.True(settings.ClearSearchOnClose);
        Assert.False(settings.ShowInTaskbar);
        Assert.Equal(20, settings.MaxRecentCharacters);
    }

    [Fact]
    public void AppearanceSettings_DefaultValues()
    {
        // Arrange & Act
        var settings = new AppearanceSettings();

        // Assert
        Assert.Equal(24, settings.FontSize);
        Assert.Equal(48, settings.ItemSize);
        Assert.Equal("light", settings.Theme);
    }

    [Fact]
    public void UserSettings_DefaultValues()
    {
        // Arrange & Act
        var settings = new UserSettings();

        // Assert
        Assert.NotNull(settings.Hotkey);
        Assert.NotNull(settings.Behavior);
        Assert.NotNull(settings.Appearance);
    }
}
