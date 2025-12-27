using UnicodeSearcher.Models;
using UnicodeSearcher.Plugins.Core;

namespace UnicodeSearcher.Plugins.Unicode;

/// <summary>
/// 유니코드 문자 검색 결과
/// </summary>
public class UnicodeSearchResult : ISearchResult
{
    private readonly UnicodeCharacter _character;

    public UnicodeSearchResult(UnicodeCharacter character)
    {
        _character = character;
    }

    /// <summary>
    /// 원본 UnicodeCharacter
    /// </summary>
    public UnicodeCharacter Character => _character;

    /// <inheritdoc/>
    public string Id => _character.Codepoint;

    /// <inheritdoc/>
    public string Title => _character.Char;

    /// <inheritdoc/>
    public string? Description => $"{_character.Name} ({_character.Codepoint})";

    /// <inheritdoc/>
    public SearchResultType Type => SearchResultType.Text;

    /// <inheritdoc/>
    public object? Preview => _character.Char;

    /// <inheritdoc/>
    public bool CanFavorite => true;

    /// <inheritdoc/>
    public Task<ClipboardContent> GetClipboardContentAsync()
    {
        return Task.FromResult(ClipboardContent.FromText(_character.Char));
    }
}
