using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 문자 검색 서비스 구현
/// </summary>
public class SearchService : ISearchService
{
    public IReadOnlyList<UnicodeCharacter> Search(string query, IEnumerable<UnicodeCharacter> characters)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return characters.ToList();
        }

        var q = query.Trim();
        var qLower = q.ToLowerInvariant();

        // 코드포인트 검색 (U+XXXX 또는 XXXX 형식)
        if (IsCodepointQuery(q, out var codepoint))
        {
            return characters
                .Where(c => c.Codepoint.Contains(codepoint, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // 일반 검색 (태그, 이름)
        return characters
            .Where(c => MatchesQuery(c, q, qLower))
            .OrderByDescending(c => GetRelevanceScore(c, q, qLower))
            .ThenByDescending(c => c.Frequency)
            .ToList();
    }

    private static bool IsCodepointQuery(string query, out string codepoint)
    {
        codepoint = query;

        // U+XXXX 형식
        if (query.StartsWith("U+", StringComparison.OrdinalIgnoreCase) ||
            query.StartsWith("u+", StringComparison.OrdinalIgnoreCase))
        {
            codepoint = query[2..];
            return codepoint.Length >= 2 && codepoint.All(c => Uri.IsHexDigit(c));
        }

        // 0xXXXX 형식
        if (query.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            codepoint = query[2..];
            return codepoint.Length >= 2 && codepoint.All(c => Uri.IsHexDigit(c));
        }

        // 순수 16진수 (4자리 이상)
        if (query.Length >= 4 && query.All(c => Uri.IsHexDigit(c)))
        {
            codepoint = query;
            return true;
        }

        return false;
    }

    private static bool MatchesQuery(UnicodeCharacter character, string query, string queryLower)
    {
        // 정확히 일치하는 문자
        if (character.Char == query)
        {
            return true;
        }

        // 한글 태그 검색
        if (character.TagsKo.Any(tag => tag.Contains(query)))
        {
            return true;
        }

        // 영어 태그 검색 (대소문자 무시)
        if (character.TagsEn.Any(tag => tag.Contains(queryLower, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // 유니코드 이름 검색 (대소문자 무시)
        if (character.Name.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 카테고리 검색
        if (character.Category.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static int GetRelevanceScore(UnicodeCharacter character, string query, string queryLower)
    {
        int score = 0;

        // 정확히 일치하는 문자
        if (character.Char == query)
        {
            score += 1000;
        }

        // 한글 태그 정확히 일치
        if (character.TagsKo.Any(tag => tag == query))
        {
            score += 500;
        }

        // 한글 태그로 시작
        if (character.TagsKo.Any(tag => tag.StartsWith(query)))
        {
            score += 300;
        }

        // 한글 태그 포함
        if (character.TagsKo.Any(tag => tag.Contains(query)))
        {
            score += 100;
        }

        // 영어 태그 정확히 일치
        if (character.TagsEn.Any(tag => tag.Equals(queryLower, StringComparison.OrdinalIgnoreCase)))
        {
            score += 500;
        }

        // 영어 태그로 시작
        if (character.TagsEn.Any(tag => tag.StartsWith(queryLower, StringComparison.OrdinalIgnoreCase)))
        {
            score += 300;
        }

        // 영어 태그 포함
        if (character.TagsEn.Any(tag => tag.Contains(queryLower, StringComparison.OrdinalIgnoreCase)))
        {
            score += 100;
        }

        // 유니코드 이름 포함
        if (character.Name.Contains(queryLower, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        return score;
    }
}
