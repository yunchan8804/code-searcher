namespace UnicodeSearcher.Plugins.Core;

/// <summary>
/// 플러그인 기본 인터페이스
/// 모든 플러그인이 구현해야 하는 최소 인터페이스
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 플러그인 고유 ID (예: "unicode", "gif", "snippet")
    /// 영문 소문자, 하이픈만 허용
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 표시 이름 (예: "유니코드", "GIF", "스니펫")
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 플러그인 설명
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 플러그인 버전
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 탭/버튼에 표시할 아이콘 (이모지 또는 Segoe MDL2 Assets 문자)
    /// </summary>
    string Icon { get; }

    /// <summary>
    /// 플러그인 활성화 상태
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// 플러그인 우선순위 (낮을수록 먼저 표시)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 플러그인 초기화 (앱 시작 시 호출)
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 플러그인 정리 (앱 종료 시 호출)
    /// </summary>
    Task ShutdownAsync();
}
