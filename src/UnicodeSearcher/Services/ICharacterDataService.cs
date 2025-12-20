using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 문자 데이터 로드 및 관리 서비스 인터페이스
/// </summary>
public interface ICharacterDataService
{
    /// <summary>
    /// 모든 문자 목록
    /// </summary>
    IReadOnlyList<UnicodeCharacter> Characters { get; }

    /// <summary>
    /// 모든 카테고리 목록
    /// </summary>
    IReadOnlyList<Category> Categories { get; }

    /// <summary>
    /// 데이터 로드 완료 여부
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// 데이터 로드
    /// </summary>
    Task LoadDataAsync();

    /// <summary>
    /// 특정 카테고리의 문자 목록 조회
    /// </summary>
    IReadOnlyList<UnicodeCharacter> GetCharactersByCategory(string categoryId);
}
