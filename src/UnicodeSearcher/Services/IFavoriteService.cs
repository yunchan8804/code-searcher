using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 즐겨찾기 서비스 인터페이스
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// 즐겨찾기 목록 (레거시 - 유니코드만)
    /// </summary>
    IReadOnlySet<string> Favorites { get; }

    /// <summary>
    /// 즐겨찾기 아이템 목록 (통합)
    /// </summary>
    IReadOnlyList<QuickAccessItem> FavoriteItems { get; }

    /// <summary>
    /// 즐겨찾기 여부 확인 (유니코드)
    /// </summary>
    bool IsFavorite(string character);

    /// <summary>
    /// 즐겨찾기 여부 확인 (통합)
    /// </summary>
    bool IsFavoriteItem(QuickAccessItem item);

    /// <summary>
    /// 즐겨찾기 토글 (추가/제거) - 유니코드
    /// </summary>
    void ToggleFavorite(string character);

    /// <summary>
    /// 즐겨찾기 토글 (통합)
    /// </summary>
    void ToggleFavoriteItem(QuickAccessItem item);

    /// <summary>
    /// 즐겨찾기 추가 (유니코드)
    /// </summary>
    void AddFavorite(string character);

    /// <summary>
    /// 즐겨찾기 추가 (통합)
    /// </summary>
    void AddFavoriteItem(QuickAccessItem item);

    /// <summary>
    /// 즐겨찾기 제거 (유니코드)
    /// </summary>
    void RemoveFavorite(string character);

    /// <summary>
    /// 즐겨찾기 제거 (통합)
    /// </summary>
    void RemoveFavoriteItem(QuickAccessItem item);

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
