using System.Text.Json.Serialization;

namespace UnicodeSearcher.Models;

/// <summary>
/// 최근 사용/즐겨찾기 아이템 (유니코드 + GIF 통합)
/// </summary>
public class QuickAccessItem
{
    /// <summary>
    /// 아이템 타입
    /// </summary>
    [JsonPropertyName("type")]
    public QuickAccessItemType Type { get; set; }

    /// <summary>
    /// 값 (유니코드: 문자, GIF: ID)
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 표시 텍스트 (유니코드: 문자, GIF: 제목)
    /// </summary>
    [JsonPropertyName("display")]
    public string? DisplayText { get; set; }

    /// <summary>
    /// 미리보기 URL (GIF 전용)
    /// </summary>
    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// 원본 URL (GIF 전용)
    /// </summary>
    [JsonPropertyName("fullUrl")]
    public string? FullUrl { get; set; }

    /// <summary>
    /// 플러그인 ID
    /// </summary>
    [JsonPropertyName("pluginId")]
    public string PluginId { get; set; } = "unicode";

    /// <summary>
    /// 유니코드 아이템 생성
    /// </summary>
    public static QuickAccessItem FromUnicode(string character)
    {
        return new QuickAccessItem
        {
            Type = QuickAccessItemType.Unicode,
            Value = character,
            DisplayText = character,
            PluginId = "unicode"
        };
    }

    /// <summary>
    /// GIF 아이템 생성
    /// </summary>
    public static QuickAccessItem FromGif(string id, string? title, string? previewUrl, string? fullUrl)
    {
        return new QuickAccessItem
        {
            Type = QuickAccessItemType.Gif,
            Value = id,
            DisplayText = title ?? "GIF",
            PreviewUrl = previewUrl,
            FullUrl = fullUrl,
            PluginId = "gif"
        };
    }

    /// <summary>
    /// 고유 키 (중복 체크용)
    /// </summary>
    [JsonIgnore]
    public string UniqueKey => $"{Type}:{Value}";

    public override bool Equals(object? obj)
    {
        if (obj is QuickAccessItem other)
        {
            return UniqueKey == other.UniqueKey;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return UniqueKey.GetHashCode();
    }
}

/// <summary>
/// 아이템 타입
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuickAccessItemType
{
    Unicode,
    Gif
}
