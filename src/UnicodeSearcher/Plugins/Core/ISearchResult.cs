namespace UnicodeSearcher.Plugins.Core;

/// <summary>
/// 검색 결과 항목
/// </summary>
public interface ISearchResult
{
    /// <summary>
    /// 결과 고유 ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 표시할 제목/이름
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 부가 설명 (선택적)
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// 결과 유형
    /// </summary>
    SearchResultType Type { get; }

    /// <summary>
    /// 미리보기 콘텐츠 (UI에 표시)
    /// - Text: 문자열
    /// - Image: BitmapSource 또는 URL 문자열
    /// </summary>
    object? Preview { get; }

    /// <summary>
    /// 즐겨찾기 가능 여부
    /// </summary>
    bool CanFavorite { get; }

    /// <summary>
    /// 클립보드에 복사할 콘텐츠 가져오기
    /// </summary>
    Task<ClipboardContent> GetClipboardContentAsync();
}

/// <summary>
/// 검색 결과 유형
/// </summary>
public enum SearchResultType
{
    /// <summary>텍스트 (유니코드 문자, 스니펫)</summary>
    Text,

    /// <summary>이미지 (GIF, 스티커)</summary>
    Image,

    /// <summary>파일 경로</summary>
    FilePath
}

/// <summary>
/// 클립보드 콘텐츠
/// </summary>
public class ClipboardContent
{
    /// <summary>텍스트 콘텐츠</summary>
    public string? Text { get; init; }

    /// <summary>이미지 콘텐츠 (BitmapSource 또는 byte[])</summary>
    public object? Image { get; init; }

    /// <summary>HTML 콘텐츠 (리치 텍스트용)</summary>
    public string? Html { get; init; }

    /// <summary>파일 경로 목록</summary>
    public IReadOnlyList<string>? FilePaths { get; init; }

    /// <summary>
    /// 텍스트 전용 클립보드 콘텐츠 생성
    /// </summary>
    public static ClipboardContent FromText(string text) => new() { Text = text };

    /// <summary>
    /// 이미지 클립보드 콘텐츠 생성
    /// </summary>
    public static ClipboardContent FromImage(object image, string? html = null)
        => new() { Image = image, Html = html };
}
