using UnicodeSearcher.Models;
using UnicodeSearcher.Plugins.Core;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.Plugins.Unicode;

/// <summary>
/// 유니코드 문자 검색 플러그인
/// 기존 유니코드 검색 기능을 플러그인 형태로 래핑
/// </summary>
public class UnicodePlugin : ISearchablePlugin
{
    private readonly ICharacterDataService _characterDataService;
    private readonly ISearchService _searchService;

    private IReadOnlyList<PluginCategory>? _categories;

    public UnicodePlugin(
        ICharacterDataService characterDataService,
        ISearchService searchService)
    {
        _characterDataService = characterDataService;
        _searchService = searchService;
    }

    #region IPlugin

    /// <inheritdoc/>
    public string Id => "unicode";

    /// <inheritdoc/>
    public string DisplayName => "유니코드";

    /// <inheritdoc/>
    public string Description => "유니코드 문자 및 기호 검색";

    /// <inheritdoc/>
    public Version Version => new(1, 0, 0);

    /// <inheritdoc/>
    public string Icon => "✦";

    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    public int Order => 0;  // 가장 먼저 표시

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        // 데이터 로드 (이미 로드되어 있으면 스킵됨)
        await _characterDataService.LoadDataAsync();

        // 카테고리 목록 생성
        _categories = _characterDataService.Categories
            .Select(c => new PluginCategory(c.Id, c.NameKo, c.Icon, c.Order))
            .ToList();
    }

    /// <inheritdoc/>
    public Task ShutdownAsync()
    {
        return Task.CompletedTask;
    }

    #endregion

    #region ISearchablePlugin

    /// <inheritdoc/>
    public string SearchPlaceholder => "문자 검색... (예: 별, star, ★)";

    /// <inheritdoc/>
    public int SearchDebounceMs => 150;

    /// <inheritdoc/>
    public IReadOnlyList<PluginCategory>? Categories => _categories;

    /// <inheritdoc/>
    public Task<IReadOnlyList<ISearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        return SearchByCategoryAsync("all", query, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ISearchResult>> SearchByCategoryAsync(
        string categoryId,
        string query,
        CancellationToken cancellationToken = default)
    {
        // 카테고리별 문자 가져오기
        var characters = categoryId == "all"
            ? _characterDataService.Characters
            : _characterDataService.GetCharactersByCategory(categoryId);

        // 검색 수행
        var searchResults = _searchService.Search(query, characters);

        // ISearchResult로 변환
        IReadOnlyList<ISearchResult> results = searchResults
            .Select(c => new UnicodeSearchResult(c))
            .ToList();

        return Task.FromResult(results);
    }

    #endregion

    #region 추가 헬퍼 메서드

    /// <summary>
    /// UnicodeCharacter를 직접 반환하는 검색 (기존 호환성)
    /// </summary>
    public IReadOnlyList<UnicodeCharacter> SearchCharacters(string query, string? categoryId = null)
    {
        var characters = string.IsNullOrEmpty(categoryId) || categoryId == "all"
            ? _characterDataService.Characters
            : _characterDataService.GetCharactersByCategory(categoryId);

        return _searchService.Search(query, characters);
    }

    /// <summary>
    /// 모든 문자 가져오기
    /// </summary>
    public IReadOnlyList<UnicodeCharacter> GetAllCharacters() =>
        _characterDataService.Characters;

    /// <summary>
    /// 카테고리별 문자 가져오기
    /// </summary>
    public IReadOnlyList<UnicodeCharacter> GetCharactersByCategory(string categoryId) =>
        _characterDataService.GetCharactersByCategory(categoryId);

    /// <summary>
    /// 기존 Category 목록 가져오기 (UI 호환성)
    /// </summary>
    public IReadOnlyList<Category> GetOriginalCategories() =>
        _characterDataService.Categories;

    #endregion
}
