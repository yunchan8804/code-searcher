using System.IO;

namespace UnicodeSearcher.Helpers;

public static class DebugLogger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        "UnicodeSearcher_Debug.log");

    private static readonly object _lock = new();

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        System.Diagnostics.Debug.WriteLine(line);

        lock (_lock)
        {
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }
    }
}
