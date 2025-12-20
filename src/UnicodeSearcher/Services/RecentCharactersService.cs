using System.IO;
using System.Text.Json;

namespace UnicodeSearcher.Services;

/// <summary>
/// 최근 사용 문자 서비스 구현
/// </summary>
public class RecentCharactersService : IRecentCharactersService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnicodeSearcher");

    private static readonly string RecentFilePath = Path.Combine(AppDataFolder, "recent.json");

    private readonly List<string> _recentCharacters = [];
    private int _maxCount = 20;

    public IReadOnlyList<string> RecentCharacters => _recentCharacters.AsReadOnly();

    public event EventHandler? RecentCharactersChanged;

    /// <summary>
    /// 최대 저장 개수 설정
    /// </summary>
    public void SetMaxCount(int maxCount)
    {
        _maxCount = Math.Max(1, Math.Min(100, maxCount));

        // 기존 목록이 최대 개수를 초과하면 정리
        while (_recentCharacters.Count > _maxCount)
        {
            _recentCharacters.RemoveAt(_recentCharacters.Count - 1);
        }
    }

    public void AddCharacter(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        // 이미 있으면 제거 (나중에 맨 앞에 추가)
        _recentCharacters.Remove(character);

        // 맨 앞에 추가
        _recentCharacters.Insert(0, character);

        // 최대 개수 초과 시 마지막 제거
        while (_recentCharacters.Count > _maxCount)
        {
            _recentCharacters.RemoveAt(_recentCharacters.Count - 1);
        }

        RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _recentCharacters.Clear();
        RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsync()
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(_recentCharacters);
            await File.WriteAllTextAsync(RecentFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save recent characters: {ex.Message}");
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(RecentFilePath))
            {
                var json = await File.ReadAllTextAsync(RecentFilePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);

                if (list != null)
                {
                    _recentCharacters.Clear();
                    _recentCharacters.AddRange(list.Take(_maxCount));
                    RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load recent characters: {ex.Message}");
        }
    }
}
