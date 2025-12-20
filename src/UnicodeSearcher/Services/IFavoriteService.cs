namespace UnicodeSearcher.Services;

/// <summary>
/// 즐겨찾기 서비스 인터페이스
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// 즐겨찾기 목록
    /// </summary>
    IReadOnlySet<string> Favorites { get; }

    /// <summary>
    /// 즐겨찾기 여부 확인
    /// </summary>
    bool IsFavorite(string character);

    /// <summary>
    /// 즐겨찾기 토글 (추가/제거)
    /// </summary>
    void ToggleFavorite(string character);

    /// <summary>
    /// 즐겨찾기 추가
    /// </summary>
    void AddFavorite(string character);

    /// <summary>
    /// 즐겨찾기 제거
    /// </summary>
    void RemoveFavorite(string character);

    /// <summary>
    /// 파일에 저장
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 파일에서 로드
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 즐겨찾기가 변경되었을 때 발생
    /// </summary>
    event EventHandler? FavoritesChanged;
}
