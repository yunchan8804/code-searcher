using UnicodeSearcher.Models;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.Tests.Services;

public class SearchServiceTests
{
    private readonly SearchService _searchService;
    private readonly List<UnicodeCharacter> _testCharacters;

    public SearchServiceTests()
    {
        _searchService = new SearchService();
        _testCharacters = new List<UnicodeCharacter>
        {
            new() { Char = "★", Name = "Black Star", Codepoint = "U+2605", TagsKo = ["별"], TagsEn = ["star", "favorite"] },
            new() { Char = "♥", Name = "Black Heart Suit", Codepoint = "U+2665", TagsKo = ["하트"], TagsEn = ["heart", "love"] },
            new() { Char = "→", Name = "Rightwards Arrow", Codepoint = "U+2192", TagsKo = ["화살표"], TagsEn = ["arrow", "right"] },
            new() { Char = "←", Name = "Leftwards Arrow", Codepoint = "U+2190", TagsKo = ["화살표"], TagsEn = ["arrow", "left"] },
            new() { Char = "♪", Name = "Eighth Note", Codepoint = "U+266A", TagsKo = ["음표"], TagsEn = ["music", "note"] },
        };
    }

    [Fact]
    public void Search_WithEmptyQuery_ReturnsAllCharacters()
    {
        // Arrange
        var query = "";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Equal(_testCharacters.Count, results.Count);
    }

    [Fact]
    public void Search_WithWhitespaceQuery_ReturnsAllCharacters()
    {
        // Arrange
        var query = "   ";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Equal(_testCharacters.Count, results.Count);
    }

    [Fact]
    public void Search_ByEnglishName_FindsCharacter()
    {
        // Arrange
        var query = "star";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("★", results[0].Char);
    }

    [Fact]
    public void Search_ByKoreanTag_FindsCharacter()
    {
        // Arrange
        var query = "별";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("★", results[0].Char);
    }

    [Fact]
    public void Search_ByCodepoint_FindsCharacter()
    {
        // Arrange
        var query = "U+2665";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("♥", results[0].Char);
    }

    [Fact]
    public void Search_ByPartialCodepoint_FindsCharacter()
    {
        // Arrange
        var query = "2665";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("♥", results[0].Char);
    }

    [Fact]
    public void Search_ByCharacter_FindsExactMatch()
    {
        // Arrange
        var query = "★";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("★", results[0].Char);
    }

    [Fact]
    public void Search_ByCommonTag_FindsMultipleCharacters()
    {
        // Arrange
        var query = "arrow";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, c => c.Char == "→");
        Assert.Contains(results, c => c.Char == "←");
    }

    [Fact]
    public void Search_CaseInsensitive_FindsCharacter()
    {
        // Arrange
        var query = "HEART";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Single(results);
        Assert.Equal("♥", results[0].Char);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var query = "없는문자";

        // Act
        var results = _searchService.Search(query, _testCharacters);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_WithNullCharacters_ThrowsArgumentNullException()
    {
        // Arrange
        var query = "test";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _searchService.Search(query, null!));
    }
}
