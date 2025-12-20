using System.IO;
using Microsoft.Win32;

namespace UnicodeSearcher.Services;

/// <summary>
/// Windows 자동 시작 관리 서비스
/// 레지스트리를 사용하여 Windows 시작 시 자동 실행을 관리
/// </summary>
public class StartupService : IStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "UnicodeSearcher";

    public bool IsRegistered => CheckRegistration();

    public void Register(bool startMinimized = true)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to open Run registry key");
                return;
            }

            var exePath = GetExecutablePath();
            var arguments = startMinimized ? "--minimized" : "";
            var command = string.IsNullOrEmpty(arguments)
                ? $"\"{exePath}\""
                : $"\"{exePath}\" {arguments}";

            key.SetValue(AppName, command);
            System.Diagnostics.Debug.WriteLine($"Registered startup: {command}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to register startup: {ex.Message}");
        }
    }

    public void Unregister()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to open Run registry key");
                return;
            }

            // 값이 존재하는 경우에만 삭제
            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
                System.Diagnostics.Debug.WriteLine("Unregistered startup");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to unregister startup: {ex.Message}");
        }
    }

    public bool CheckRegistration()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to check startup registration: {ex.Message}");
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        // 싱글파일 앱에서도 동작하는 방식
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(processPath))
        {
            return processPath;
        }

        // 폴백: AppContext.BaseDirectory 사용
        return Path.Combine(AppContext.BaseDirectory, "UnicodeSearcher.exe");
    }
}
