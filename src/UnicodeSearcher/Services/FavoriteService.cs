using System.IO;
using System.Text.Json;

namespace UnicodeSearcher.Services;

/// <summary>
/// 즐겨찾기 서비스 구현
/// </summary>
public class FavoriteService : IFavoriteService
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "UnicodeSearcher");

    private static readonly string FavoritesFilePath = Path.Combine(AppDataFolder, "favorites.json");

    private readonly HashSet<string> _favorites = [];

    public IReadOnlySet<string> Favorites => _favorites;

    public event EventHandler? FavoritesChanged;

    public bool IsFavorite(string character)
    {
        return _favorites.Contains(character);
    }

    public void ToggleFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        if (_favorites.Contains(character))
        {
            _favorites.Remove(character);
        }
        else
        {
            _favorites.Add(character);
        }

        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        if (_favorites.Add(character))
        {
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveFavorite(string character)
    {
        if (string.IsNullOrEmpty(character)) return;

        if (_favorites.Remove(character))
        {
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);

            var json = JsonSerializer.Serialize(_favorites.ToList());
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
            if (File.Exists(FavoritesFilePath))
            {
                var json = await File.ReadAllTextAsync(FavoritesFilePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);

                if (list != null)
                {
                    _favorites.Clear();
                    foreach (var item in list)
                    {
                        _favorites.Add(item);
                    }
                    FavoritesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load favorites: {ex.Message}");
        }
    }
}
