using System.Text.Json.Serialization;

namespace UnicodeSearcher.Models;

/// <summary>
/// 유니코드 문자 정보를 나타내는 모델
/// </summary>
public record UnicodeCharacter
{
    /// <summary>
    /// 실제 문자 (예: "★")
    /// </summary>
    [JsonPropertyName("char")]
    public required string Char { get; init; }

    /// <summary>
    /// 유니코드 코드포인트 (예: "U+2605")
    /// </summary>
    [JsonPropertyName("codepoint")]
    public required string Codepoint { get; init; }

    /// <summary>
    /// 유니코드 공식 이름 (예: "BLACK STAR")
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// 유니코드 블록 (예: "Miscellaneous Symbols")
    /// </summary>
    [JsonPropertyName("block")]
    public string Block { get; init; } = string.Empty;

    /// <summary>
    /// 한글 검색 태그 (예: ["별", "검은별"])
    /// </summary>
    [JsonPropertyName("tags_ko")]
    public string[] TagsKo { get; init; } = [];

    /// <summary>
    /// 영어 검색 태그 (예: ["star", "black star"])
    /// </summary>
    [JsonPropertyName("tags_en")]
    public string[] TagsEn { get; init; } = [];

    /// <summary>
    /// 카테고리 ID (예: "star")
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// 서브카테고리 (예: "solid")
    /// </summary>
    [JsonPropertyName("subcategory")]
    public string Subcategory { get; init; } = string.Empty;

    /// <summary>
    /// 사용 빈도 (정렬용, 높을수록 자주 사용)
    /// </summary>
    [JsonPropertyName("frequency")]
    public int Frequency { get; init; } = 50;
}
