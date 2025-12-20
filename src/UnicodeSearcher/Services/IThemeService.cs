namespace UnicodeSearcher.Services;

/// <summary>
/// 테마 모드
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// 시스템 설정 따르기
    /// </summary>
    System,

    /// <summary>
    /// 라이트 테마
    /// </summary>
    Light,

    /// <summary>
    /// 다크 테마
    /// </summary>
    Dark
}

/// <summary>
/// 테마 서비스 인터페이스
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// 현재 테마 모드
    /// </summary>
    ThemeMode CurrentMode { get; }

    /// <summary>
    /// 실제 적용된 테마 (System인 경우 실제 테마)
    /// </summary>
    ThemeMode ActualTheme { get; }

    /// <summary>
    /// 테마 변경
    /// </summary>
    void SetTheme(ThemeMode mode);

    /// <summary>
    /// 테마가 변경되었을 때 발생
    /// </summary>
    event EventHandler<ThemeMode>? ThemeChanged;
}
