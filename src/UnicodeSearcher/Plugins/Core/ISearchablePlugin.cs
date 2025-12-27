namespace UnicodeSearcher.Plugins.Core;

/// <summary>
/// 검색 기능을 제공하는 플러그인
/// </summary>
public interface ISearchablePlugin : IPlugin
{
    /// <summary>
    /// 검색창 플레이스홀더 텍스트
    /// </summary>
    string SearchPlaceholder { get; }

    /// <summary>
    /// 검색 debounce 시간 (ms)
    /// API 호출이 필요한 경우 500ms 이상 권장
    /// </summary>
    int SearchDebounceMs { get; }

    /// <summary>
    /// 카테고리 목록 (선택적)
    /// null이면 카테고리 없음
    /// </summary>
    IReadOnlyList<PluginCategory>? Categories { get; }

    /// <summary>
    /// 검색 수행
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>검색 결과 목록</returns>
    Task<IReadOnlyList<ISearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 카테고리별 검색
    /// </summary>
    /// <param name="categoryId">카테고리 ID ("all"이면 전체)</param>
    /// <param name="query">검색어</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>검색 결과 목록</returns>
    Task<IReadOnlyList<ISearchResult>> SearchByCategoryAsync(
        string categoryId,
        string query,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 플러그인 카테고리
/// </summary>
public record PluginCategory(
    string Id,
    string Name,
    string? Icon = null,
    int Order = 0
);
