using System.Text.Json.Serialization;

namespace UnicodeSearcher.Models;

/// <summary>
/// characters.json 파일의 루트 구조
/// </summary>
public record CharacterData
{
    /// <summary>
    /// 데이터 버전
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// 마지막 업데이트 날짜
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; init; } = string.Empty;

    /// <summary>
    /// 문자 목록
    /// </summary>
    [JsonPropertyName("characters")]
    public UnicodeCharacter[] Characters { get; init; } = [];

    /// <summary>
    /// 카테고리 목록
    /// </summary>
    [JsonPropertyName("categories")]
    public Category[] Categories { get; init; } = [];
}
