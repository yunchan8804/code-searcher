using System.IO;
using System.Text.Json;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 즐겨찾기 서비스 구현
/// </summary>
public class FavoriteService : IFavoriteService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnicodeSearcher");

    private static readonly string FavoritesFilePath = Path.Combine(AppDataFolder, "favorites_v2.json");
    private static readonly string LegacyFavoritesFilePath = Path.Combine(AppDataFolder, "favorites.json");

    private readonly List<QuickAccessItem> _favoriteItems = [];
    private readonly HashSet<string> _favoriteKeys = []; // 빠른 조회용

    /// <summary>
    /// 즐겨찾기 목록 (레거시 호환 - 유니코드만)
    /// </summary>
    public IReadOnlySet<string> Favorites =>
        _favoriteItems
            .Where(i => i.Type == QuickAccessItemType.Unicode)
            .Select(i => i.Value)
            .ToHashSet();

    /// <summary>
    /// 즐겨찾기 아이템 목록 (통합)
    /// </summary>
    public IReadOnlyList<QuickAccessItem> FavoriteItems => _favoriteItems.AsReadOnly();

    public event EventHandler? FavoritesChanged;

    public bool IsFavorite(string character)
    {
        var key = $"{QuickAccessItemType.Unicode}:{character}";
        return _favoriteKeys.Contains(key);
    }

    public bool IsFavoriteItem(QuickAccessItem item)
    {
        return _favoriteKeys.Contains(item.UniqueKey);
    }

    public void ToggleFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        var item = QuickAccessItem.FromUnicode(character);
        ToggleFavoriteItem(item);
    }

    public void ToggleFavoriteItem(QuickAccessItem item)
    {
        if (item == null) return;

        if (_favoriteKeys.Contains(item.UniqueKey))
        {
            RemoveFavoriteItem(item);
        }
        else
        {
            AddFavoriteItem(item);
        }
    }

    public void AddFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        var item = QuickAccessItem.FromUnicode(character);
        AddFavoriteItem(item);
    }

    public void AddFavoriteItem(QuickAccessItem item)
    {
        if (item == null) return;

        if (_favoriteKeys.Add(item.UniqueKey))
        {
            _favoriteItems.Add(item);
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        var item = QuickAccessItem.FromUnicode(character);
        RemoveFavoriteItem(item);
    }

    public void RemoveFavoriteItem(QuickAccessItem item)
    {
        if (item == null) return;

        if (_favoriteKeys.Remove(item.UniqueKey))
        {
            var existing = _favoriteItems.FirstOrDefault(i => i.UniqueKey == item.UniqueKey);
            if (existing != null)
            {
                _favoriteItems.Remove(existing);
            }
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(_favoriteItems, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(FavoritesFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save favorites: {ex.Message}");
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            // 새 형식 파일이 있으면 로드
            if (File.Exists(FavoritesFilePath))
            {
                var json = await File.ReadAllTextAsync(FavoritesFilePath);
                var list = JsonSerializer.Deserialize<List<QuickAccessItem>>(json);

                if (list != null)
                {
                    _favoriteItems.Clear();
                    _favoriteKeys.Clear();
                    foreach (var item in list)
                    {
                        _favoriteItems.Add(item);
                        _favoriteKeys.Add(item.UniqueKey);
                    }
                    FavoritesChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            // 레거시 파일 마이그레이션
            if (File.Exists(LegacyFavoritesFilePath))
            {
                var json = await File.ReadAllTextAsync(LegacyFavoritesFilePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);

                if (list != null)
                {
                    _favoriteItems.Clear();
                    _favoriteKeys.Clear();
                    foreach (var character in list)
                    {
                        var item = QuickAccessItem.FromUnicode(character);
                        _favoriteItems.Add(item);
                        _favoriteKeys.Add(item.UniqueKey);
                    }
                    FavoritesChanged?.Invoke(this, EventArgs.Empty);

                    // 새 형식으로 저장
                    await SaveAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load favorites: {ex.Message}");
        }
    }
}
