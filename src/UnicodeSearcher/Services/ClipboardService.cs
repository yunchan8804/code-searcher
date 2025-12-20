using System.Windows;

namespace UnicodeSearcher.Services;

/// <summary>
/// 클립보드 서비스 구현
/// </summary>
public class ClipboardService : IClipboardService
{
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 50;

    public bool Copy(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clipboard failed: {ex.Message}");
            return false;
        }
    }
}
