using System.IO;
using UnicodeSearcher.Plugins.Core;

namespace UnicodeSearcher.Plugins.Gif;

/// <summary>
/// GIF 검색 결과
/// </summary>
public class GifSearchResult : ISearchResult
{
    private readonly TenorGif _gif;
    private readonly TenorApiClient _apiClient;

    public GifSearchResult(TenorGif gif, TenorApiClient apiClient)
    {
        _gif = gif;
        _apiClient = apiClient;
    }

    /// <inheritdoc/>
    public string Id => _gif.Id;

    /// <inheritdoc/>
    public string Title => string.IsNullOrEmpty(_gif.Title)
        ? _gif.ContentDescription
        : _gif.Title;

    /// <inheritdoc/>
    public string? Description => _gif.ContentDescription;

    /// <inheritdoc/>
    public SearchResultType Type => SearchResultType.Image;

    /// <inheritdoc/>
    public object? Preview => PreviewUrl;

    /// <inheritdoc/>
    public bool CanFavorite => false;  // GIF는 즐겨찾기 지원 안 함

    /// <summary>
    /// 미리보기 URL (tinygif)
    /// </summary>
    public string? PreviewUrl => _gif.TinyGifUrl;

    /// <summary>
    /// 원본 GIF URL
    /// </summary>
    public string? FullUrl => _gif.GifUrl;

    /// <inheritdoc/>
    public async Task<ClipboardContent> GetClipboardContentAsync()
    {
        // GIF URL을 클립보드에 복사 (대부분의 앱에서 바로 사용 가능)
        var gifUrl = _gif.GifUrl ?? _gif.TinyGifUrl;

        if (string.IsNullOrEmpty(gifUrl))
        {
            return ClipboardContent.FromText(_gif.Url);
        }

        // GIF 이미지 다운로드 시도
        try
        {
            var gifData = await _apiClient.DownloadGifAsync(gifUrl);

            if (gifData != null)
            {
                // 파일로 임시 저장 후 파일 경로로 클립보드에 복사
                var tempPath = Path.Combine(Path.GetTempPath(), $"tenor_{_gif.Id}.gif");
                await File.WriteAllBytesAsync(tempPath, gifData);

                return new ClipboardContent
                {
                    FilePaths = [tempPath],
                    Text = gifUrl,  // 텍스트로도 URL 복사
                    Html = $"<img src=\"{gifUrl}\" alt=\"{Title}\" />"
                };
            }
        }
        catch
        {
            // 다운로드 실패 시 URL만 복사
        }

        return new ClipboardContent
        {
            Text = gifUrl,
            Html = $"<img src=\"{gifUrl}\" alt=\"{Title}\" />"
        };
    }
}
