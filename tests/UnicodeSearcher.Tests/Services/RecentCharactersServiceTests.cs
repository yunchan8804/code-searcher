using UnicodeSearcher.Services;

namespace UnicodeSearcher.Tests.Services;

public class RecentCharactersServiceTests
{
    [Fact]
    public void AddCharacter_AddsToRecentList()
    {
        // Arrange
        var service = new RecentCharactersService();
        var character = "★";

        // Act
        service.AddCharacter(character);

        // Assert
        Assert.Contains(character, service.RecentCharacters);
    }

    [Fact]
    public void AddCharacter_MostRecentFirst()
    {
        // Arrange
        var service = new RecentCharactersService();

        // Act
        service.AddCharacter("★");
        service.AddCharacter("♥");
        service.AddCharacter("→");

        // Assert
        Assert.Equal("→", service.RecentCharacters[0]);
        Assert.Equal("♥", service.RecentCharacters[1]);
        Assert.Equal("★", service.RecentCharacters[2]);
    }

    [Fact]
    public void AddCharacter_DuplicateMovesToFront()
    {
        // Arrange
        var service = new RecentCharactersService();
        service.AddCharacter("★");
        service.AddCharacter("♥");
        service.AddCharacter("→");

        // Act - Add duplicate
        service.AddCharacter("★");

        // Assert
        Assert.Equal("★", service.RecentCharacters[0]);
        Assert.Equal(3, service.RecentCharacters.Count);
    }

    [Fact]
    public void AddCharacter_RespectsMaxLimit()
    {
        // Arrange
        var maxCount = 20;
        var service = new RecentCharactersService();

        // Act - Add more than max
        for (int i = 0; i < 25; i++)
        {
            service.AddCharacter($"char{i}");
        }

        // Assert
        Assert.Equal(maxCount, service.RecentCharacters.Count);
        Assert.Equal("char24", service.RecentCharacters[0]); // Most recent
        Assert.Equal("char5", service.RecentCharacters[maxCount - 1]); // Oldest kept
    }

    [Fact]
    public void RecentCharactersChanged_EventFired_WhenAdded()
    {
        // Arrange
        var service = new RecentCharactersService();
        var eventFired = false;
        service.RecentCharactersChanged += (_, _) => eventFired = true;

        // Act
        service.AddCharacter("★");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Clear_RemovesAllCharacters()
    {
        // Arrange
        var service = new RecentCharactersService();
        service.AddCharacter("★");
        service.AddCharacter("♥");

        // Act
        service.Clear();

        // Assert
        Assert.Empty(service.RecentCharacters);
    }

    [Fact]
    public void RecentCharacters_IsReadOnly()
    {
        // Arrange
        var service = new RecentCharactersService();
        service.AddCharacter("★");

        // Act & Assert
        var recent = service.RecentCharacters;
        Assert.IsAssignableFrom<IReadOnlyList<string>>(recent);
    }
}
