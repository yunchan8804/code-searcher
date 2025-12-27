using System.IO;
using System.Text.Json;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 최근 사용 문자 서비스 구현
/// </summary>
public class RecentCharactersService : IRecentCharactersService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnicodeSearcher");

    private static readonly string RecentFilePath = Path.Combine(AppDataFolder, "recent_v2.json");
    private static readonly string LegacyRecentFilePath = Path.Combine(AppDataFolder, "recent.json");

    private readonly List<QuickAccessItem> _recentItems = [];
    private int _maxCount = 20;

    /// <summary>
    /// 최근 사용 문자 목록 (레거시 호환 - 유니코드만)
    /// </summary>
    public IReadOnlyList<string> RecentCharacters =>
        _recentItems
            .Where(i => i.Type == QuickAccessItemType.Unicode)
            .Select(i => i.Value)
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// 최근 사용 아이템 목록 (통합)
    /// </summary>
    public IReadOnlyList<QuickAccessItem> RecentItems => _recentItems.AsReadOnly();

    public event EventHandler? RecentCharactersChanged;

    /// <summary>
    /// 최대 저장 개수 설정
    /// </summary>
    public void SetMaxCount(int maxCount)
    {
        _maxCount = Math.Max(1, Math.Min(100, maxCount));

        // 기존 목록이 최대 개수를 초과하면 정리
        while (_recentItems.Count > _maxCount)
        {
            _recentItems.RemoveAt(_recentItems.Count - 1);
        }
    }

    public void AddCharacter(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        var item = QuickAccessItem.FromUnicode(character);
        AddItem(item);
    }

    public void AddItem(QuickAccessItem item)
    {
        if (item == null) return;

        // 이미 있으면 제거 (나중에 맨 앞에 추가)
        var existing = _recentItems.FirstOrDefault(i => i.UniqueKey == item.UniqueKey);
        if (existing != null)
        {
            _recentItems.Remove(existing);
        }

        // 맨 앞에 추가
        _recentItems.Insert(0, item);

        // 최대 개수 초과 시 마지막 제거
        while (_recentItems.Count > _maxCount)
        {
            _recentItems.RemoveAt(_recentItems.Count - 1);
        }

        RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _recentItems.Clear();
        RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsync()
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(_recentItems, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(RecentFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save recent items: {ex.Message}");
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            // 새 형식 파일이 있으면 로드
            if (File.Exists(RecentFilePath))
            {
                var json = await File.ReadAllTextAsync(RecentFilePath);
                var list = JsonSerializer.Deserialize<List<QuickAccessItem>>(json);

                if (list != null)
                {
                    _recentItems.Clear();
                    _recentItems.AddRange(list.Take(_maxCount));
                    RecentCharactersChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            // 레거시 파일 마이그레이션
            if (File.Exists(LegacyRecentFilePath))
            {
                var json = await File.ReadAllTextAsync(LegacyRecentFilePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);

                if (list != null)
                {
                    _recentItems.Clear();
                    foreach (var character in list.Take(_maxCount))
                    {
                        _recentItems.Add(QuickAccessItem.FromUnicode(character));
                    }
                    RecentCharactersChanged?.Invoke(this, EventArgs.Empty);

                    // 새 형식으로 저장
                    await SaveAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load recent items: {ex.Message}");
        }
    }
}
