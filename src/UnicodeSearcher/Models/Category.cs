using System.Text.Json.Serialization;

namespace UnicodeSearcher.Models;

/// <summary>
/// 문자 카테고리 정보를 나타내는 모델
/// </summary>
public record Category
{
    /// <summary>
    /// 카테고리 ID (예: "star")
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// 한글 이름 (예: "별")
    /// </summary>
    [JsonPropertyName("name_ko")]
    public required string NameKo { get; init; }

    /// <summary>
    /// 영어 이름 (예: "Stars")
    /// </summary>
    [JsonPropertyName("name_en")]
    public required string NameEn { get; init; }

    /// <summary>
    /// 카테고리 아이콘 문자 (예: "⭐")
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; init; } = string.Empty;

    /// <summary>
    /// 정렬 순서 (낮을수록 앞에 표시)
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; init; } = 100;
}
