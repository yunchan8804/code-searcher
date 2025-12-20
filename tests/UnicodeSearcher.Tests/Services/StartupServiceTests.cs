using UnicodeSearcher.Services;
using Microsoft.Win32;

namespace UnicodeSearcher.Tests.Services;

/// <summary>
/// StartupService unit tests
/// - Registry registration/unregistration logic
/// - Startup state verification
/// </summary>
public class StartupServiceTests : IDisposable
{
    private readonly StartupService _startupService;
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "UnicodeSearcher";

    public StartupServiceTests()
    {
        _startupService = new StartupService();
        // 테스트 시작 전 정리
        CleanupRegistryEntry();
    }

    [Fact]
    public void IsRegistered_WhenNotRegistered_ReturnsFalse()
    {
        // Arrange - 레지스트리 엔트리 정리
        CleanupRegistryEntry();

        // Act
        var isRegistered = _startupService.IsRegistered;

        // Assert
        Assert.False(isRegistered);
    }

    [Fact]
    public void Register_CreatesRegistryEntry()
    {
        // Arrange
        CleanupRegistryEntry();

        // Act
        _startupService.Register(startMinimized: true);

        // Assert
        Assert.True(_startupService.IsRegistered);
    }

    [Fact]
    public void Register_WithMinimized_IncludesMinimizedArgument()
    {
        // Arrange
        CleanupRegistryEntry();

        // Act
        _startupService.Register(startMinimized: true);

        // Assert
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        var value = key?.GetValue(AppName)?.ToString();
        Assert.NotNull(value);
        Assert.Contains("--minimized", value);
    }

    [Fact]
    public void Register_WithoutMinimized_DoesNotIncludeMinimizedArgument()
    {
        // Arrange
        CleanupRegistryEntry();

        // Act
        _startupService.Register(startMinimized: false);

        // Assert
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        var value = key?.GetValue(AppName)?.ToString();
        Assert.NotNull(value);
        Assert.DoesNotContain("--minimized", value);
    }

    [Fact]
    public void Unregister_RemovesRegistryEntry()
    {
        // Arrange
        _startupService.Register(startMinimized: true);
        Assert.True(_startupService.IsRegistered);

        // Act
        _startupService.Unregister();

        // Assert
        Assert.False(_startupService.IsRegistered);
    }

    [Fact]
    public void Unregister_WhenNotRegistered_DoesNotThrow()
    {
        // Arrange
        CleanupRegistryEntry();

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _startupService.Unregister());
        Assert.Null(exception);
    }

    [Fact]
    public void CheckRegistration_ReturnsCorrectState()
    {
        // Arrange
        CleanupRegistryEntry();

        // Act & Assert - before registration
        Assert.False(_startupService.CheckRegistration());

        // Register
        _startupService.Register(startMinimized: true);

        // Assert - after registration
        Assert.True(_startupService.CheckRegistration());
    }

    [Fact]
    public void Register_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() =>
        {
            _startupService.Register(startMinimized: true);
            _startupService.Register(startMinimized: false);
            _startupService.Register(startMinimized: true);
        });

        Assert.Null(exception);
    }

    private void CleanupRegistryEntry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key?.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public void Dispose()
    {
        // 테스트 후 정리
        CleanupRegistryEntry();
    }
}
