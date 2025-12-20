using System.IO;
using System.Reflection;
using System.Text.Json;
using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 문자 데이터 로드 및 관리 서비스 구현
/// </summary>
public class CharacterDataService : ICharacterDataService
{
    private List<UnicodeCharacter> _characters = [];
    private List<Category> _categories = [];

    public IReadOnlyList<UnicodeCharacter> Characters => _characters;
    public IReadOnlyList<Category> Categories => _categories;
    public bool IsLoaded { get; private set; }

    public async Task LoadDataAsync()
    {
        if (IsLoaded) return;

        try
        {
            var json = await LoadJsonFromResourceAsync();
            var data = JsonSerializer.Deserialize<CharacterData>(json);

            if (data != null)
            {
                _characters = data.Characters
                    .OrderByDescending(c => c.Frequency)
                    .ToList();

                _categories = data.Categories
                    .OrderBy(c => c.Order)
                    .ToList();
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load character data: {ex.Message}");
            throw;
        }
    }

    public IReadOnlyList<UnicodeCharacter> GetCharactersByCategory(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId) || categoryId == "all")
        {
            return _characters;
        }

        return _characters
            .Where(c => c.Category == categoryId)
            .ToList();
    }

    private static async Task<string> LoadJsonFromResourceAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "UnicodeSearcher.Data.characters.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            // Fallback: 파일 시스템에서 로드
            var assemblyLocation = Path.GetDirectoryName(assembly.Location) ?? "";
            var filePath = Path.Combine(assemblyLocation, "Data", "characters.json");

            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }

            throw new FileNotFoundException($"Character data not found: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
