using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace UnicodeSearcher.Plugins.Gif;

/// <summary>
/// Tenor API 클라이언트
/// </summary>
public class TenorApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://tenor.googleapis.com/v2";

    public TenorApiClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// GIF 검색
    /// </summary>
    public async Task<TenorSearchResponse?> SearchAsync(
        string query,
        int limit = 20,
        string? pos = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        try
        {
            var url = $"{BaseUrl}/search?key={_apiKey}&q={Uri.EscapeDataString(query)}&limit={limit}&media_filter=gif,tinygif&contentfilter=medium";

            if (!string.IsNullOrEmpty(pos))
                url += $"&pos={pos}";

            var response = await _httpClient.GetFromJsonAsync<TenorSearchResponse>(url, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TenorApiClient] Search error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 인기 GIF 가져오기
    /// </summary>
    public async Task<TenorSearchResponse?> GetFeaturedAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BaseUrl}/featured?key={_apiKey}&limit={limit}&media_filter=gif,tinygif&contentfilter=medium";
            var response = await _httpClient.GetFromJsonAsync<TenorSearchResponse>(url, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TenorApiClient] Featured error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// GIF 데이터 다운로드
    /// </summary>
    public async Task<byte[]?> DownloadGifAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TenorApiClient] Download error: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

#region Tenor API Response Models

public class TenorSearchResponse
{
    [JsonPropertyName("results")]
    public List<TenorGif> Results { get; set; } = [];

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

public class TenorGif
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content_description")]
    public string ContentDescription { get; set; } = string.Empty;

    [JsonPropertyName("media_formats")]
    public Dictionary<string, TenorMediaFormat> MediaFormats { get; set; } = [];

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// tinygif URL (미리보기용)
    /// </summary>
    public string? TinyGifUrl => MediaFormats.TryGetValue("tinygif", out var format) ? format.Url : null;

    /// <summary>
    /// gif URL (원본)
    /// </summary>
    public string? GifUrl => MediaFormats.TryGetValue("gif", out var format) ? format.Url : null;
}

public class TenorMediaFormat
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("dims")]
    public int[] Dims { get; set; } = [];

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

#endregion
