namespace UnicodeSearcher.Services;

/// <summary>
/// 클립보드 서비스 인터페이스
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// 텍스트를 클립보드에 복사
    /// </summary>
    /// <param name="text">복사할 텍스트</param>
    /// <returns>복사 성공 여부</returns>
    bool Copy(string text);
}
