using UnicodeSearcher.Services;

namespace UnicodeSearcher.Tests.Services;

/// <summary>
/// CharacterDataService unit tests
/// - JSON loading from embedded resource
/// - UTF-8 encoding (BOM-free) validation
/// - Category filtering
/// </summary>
public class CharacterDataServiceTests
{
    [Fact]
    public async Task LoadDataAsync_LoadsCharactersSuccessfully()
    {
        // Arrange
        var service = new CharacterDataService();

        // Act
        await service.LoadDataAsync();

        // Assert
        Assert.True(service.IsLoaded);
        Assert.NotEmpty(service.Characters);
    }

    [Fact]
    public async Task LoadDataAsync_LoadsCategoriesSuccessfully()
    {
        // Arrange
        var service = new CharacterDataService();

        // Act
        await service.LoadDataAsync();

        // Assert
        Assert.NotEmpty(service.Categories);
    }

    [Fact]
    public async Task LoadDataAsync_CalledTwice_OnlyLoadsOnce()
    {
        // Arrange
        var service = new CharacterDataService();

        // Act
        await service.LoadDataAsync();
        var firstCount = service.Characters.Count;
        await service.LoadDataAsync();
        var secondCount = service.Characters.Count;

        // Assert
        Assert.Equal(firstCount, secondCount);
    }

    [Fact]
    public async Task Characters_ContainsUnicodeCharacters()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act & Assert - Check for specific Unicode characters
        Assert.Contains(service.Characters, c => c.Char == "\u2605"); // ★ BLACK STAR
        Assert.Contains(service.Characters, c => c.Char == "\u2606"); // ☆ WHITE STAR
    }

    [Fact]
    public async Task Characters_ContainsKoreanTags()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var starChar = service.Characters.FirstOrDefault(c => c.Char == "\u2605");

        // Assert
        Assert.NotNull(starChar);
        Assert.NotEmpty(starChar.TagsKo);
        Assert.Contains(starChar.TagsKo, t => t.Contains("\uBCC4")); // "별"
    }

    [Fact]
    public async Task Characters_ContainsEnglishTags()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var starChar = service.Characters.FirstOrDefault(c => c.Char == "\u2605");

        // Assert
        Assert.NotNull(starChar);
        Assert.NotEmpty(starChar.TagsEn);
        Assert.Contains(starChar.TagsEn, t => t.ToLower().Contains("star"));
    }

    [Fact]
    public async Task Characters_HaveValidCodepoints()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act & Assert
        foreach (var character in service.Characters)
        {
            Assert.NotNull(character.Codepoint);
            Assert.StartsWith("U+", character.Codepoint);
        }
    }

    [Fact]
    public async Task Characters_AreSortedByFrequency()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var characters = service.Characters.ToList();

        // Assert - Check descending order
        for (int i = 0; i < characters.Count - 1; i++)
        {
            Assert.True(characters[i].Frequency >= characters[i + 1].Frequency,
                $"Characters not sorted by frequency: {characters[i].Char} ({characters[i].Frequency}) should be >= {characters[i + 1].Char} ({characters[i + 1].Frequency})");
        }
    }

    [Fact]
    public async Task Categories_AreSortedByOrder()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var categories = service.Categories.ToList();

        // Assert - Check ascending order
        for (int i = 0; i < categories.Count - 1; i++)
        {
            Assert.True(categories[i].Order <= categories[i + 1].Order,
                $"Categories not sorted by order: {categories[i].Id} ({categories[i].Order}) should be <= {categories[i + 1].Id} ({categories[i + 1].Order})");
        }
    }

    [Fact]
    public async Task GetCharactersByCategory_WithAllCategory_ReturnsAllCharacters()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var result = service.GetCharactersByCategory("all");

        // Assert
        Assert.Equal(service.Characters.Count, result.Count);
    }

    [Fact]
    public async Task GetCharactersByCategory_WithEmptyString_ReturnsAllCharacters()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var result = service.GetCharactersByCategory("");

        // Assert
        Assert.Equal(service.Characters.Count, result.Count);
    }

    [Fact]
    public async Task GetCharactersByCategory_WithNull_ReturnsAllCharacters()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var result = service.GetCharactersByCategory(null!);

        // Assert
        Assert.Equal(service.Characters.Count, result.Count);
    }

    [Fact]
    public async Task GetCharactersByCategory_WithValidCategory_ReturnsFilteredCharacters()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var starCharacters = service.GetCharactersByCategory("star");

        // Assert
        Assert.NotEmpty(starCharacters);
        Assert.All(starCharacters, c => Assert.Equal("star", c.Category));
    }

    [Fact]
    public async Task GetCharactersByCategory_WithInvalidCategory_ReturnsEmpty()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var result = service.GetCharactersByCategory("nonexistent_category");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Categories_ContainsExpectedCategories()
    {
        // Arrange
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act & Assert
        var categoryIds = service.Categories.Select(c => c.Id).ToList();
        Assert.Contains("all", categoryIds);
        Assert.Contains("star", categoryIds);
        Assert.Contains("arrow", categoryIds);
        Assert.Contains("heart", categoryIds);
    }

    [Fact]
    public async Task WonSign_IsProperlyEncoded()
    {
        // Arrange - This tests the UTF-8 encoding fix for Korean Won sign
        var service = new CharacterDataService();
        await service.LoadDataAsync();

        // Act
        var wonChar = service.Characters.FirstOrDefault(c => c.Codepoint == "U+20A9");

        // Assert
        Assert.NotNull(wonChar);
        Assert.Equal("\u20A9", wonChar.Char); // ₩
    }
}
