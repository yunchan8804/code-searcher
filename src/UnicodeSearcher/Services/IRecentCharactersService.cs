using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 최근 사용 문자 서비스 인터페이스
/// </summary>
public interface IRecentCharactersService
{
    /// <summary>
    /// 최근 사용 문자 목록 (레거시 - 유니코드만)
    /// </summary>
    IReadOnlyList<string> RecentCharacters { get; }

    /// <summary>
    /// 최근 사용 아이템 목록 (통합 - 유니코드+GIF)
    /// </summary>
    IReadOnlyList<QuickAccessItem> RecentItems { get; }

    /// <summary>
    /// 문자 추가 (최대 개수 초과 시 오래된 것부터 제거)
    /// </summary>
    void AddCharacter(string character);

    /// <summary>
    /// 아이템 추가 (통합)
    /// </summary>
    void AddItem(QuickAccessItem item);

    /// <summary>
    /// 목록 초기화
    /// </summary>
    void Clear();

    /// <summary>
    /// 파일에 저장
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 파일에서 로드
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 목록이 변경되었을 때 발생
    /// </summary>
    event EventHandler? RecentCharactersChanged;
}
