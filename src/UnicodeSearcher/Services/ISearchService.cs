using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 문자 검색 서비스 인터페이스
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 검색어로 문자 검색
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="characters">검색 대상 문자 목록</param>
    /// <returns>검색 결과 문자 목록</returns>
    IReadOnlyList<UnicodeCharacter> Search(string query, IEnumerable<UnicodeCharacter> characters);
}
