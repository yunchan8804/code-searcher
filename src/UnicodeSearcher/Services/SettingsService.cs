using System.IO;
using System.Text.Json;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 설정 서비스 구현
/// </summary>
public class SettingsService : ISettingsService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnicodeSearcher");

    private static readonly string SettingsFilePath = Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public UserSettings Settings { get; private set; } = new();

    public event EventHandler<UserSettings>? SettingsChanged;

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = await File.ReadAllTextAsync(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
                if (settings != null)
                {
                    Settings = settings;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            Settings = new UserSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            // 폴더 생성
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            await File.WriteAllTextAsync(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public void UpdateSettings(Action<UserSettings> updateAction)
    {
        updateAction(Settings);
        SettingsChanged?.Invoke(this, Settings);
    }
}
