using UnicodeSearcher.Services;

namespace UnicodeSearcher.Tests.Services;

public class FavoriteServiceTests
{
    [Fact]
    public void IsFavorite_WhenNotAdded_ReturnsFalse()
    {
        // Arrange
        var service = new FavoriteService();
        var character = "★";

        // Act
        var result = service.IsFavorite(character);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToggleFavorite_AddsCharacterToFavorites()
    {
        // Arrange
        var service = new FavoriteService();
        var character = "★";

        // Act
        service.ToggleFavorite(character);

        // Assert
        Assert.True(service.IsFavorite(character));
        Assert.Contains(character, service.Favorites);
    }

    [Fact]
    public void ToggleFavorite_RemovesCharacterFromFavorites()
    {
        // Arrange
        var service = new FavoriteService();
        var character = "★";
        service.ToggleFavorite(character); // Add

        // Act
        service.ToggleFavorite(character); // Remove

        // Assert
        Assert.False(service.IsFavorite(character));
        Assert.DoesNotContain(character, service.Favorites);
    }

    [Fact]
    public void ToggleFavorite_MultipleCharacters_AllAdded()
    {
        // Arrange
        var service = new FavoriteService();
        var characters = new[] { "★", "♥", "→" };

        // Act
        foreach (var c in characters)
        {
            service.ToggleFavorite(c);
        }

        // Assert
        Assert.Equal(3, service.Favorites.Count);
        foreach (var c in characters)
        {
            Assert.True(service.IsFavorite(c));
        }
    }

    [Fact]
    public void FavoritesChanged_EventFired_WhenToggled()
    {
        // Arrange
        var service = new FavoriteService();
        var eventFired = false;
        service.FavoritesChanged += (_, _) => eventFired = true;

        // Act
        service.ToggleFavorite("★");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Favorites_IsReadOnly()
    {
        // Arrange
        var service = new FavoriteService();
        service.ToggleFavorite("★");

        // Act & Assert
        var favorites = service.Favorites;
        Assert.IsAssignableFrom<IReadOnlySet<string>>(favorites);
    }
}
