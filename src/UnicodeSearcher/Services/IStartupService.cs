namespace UnicodeSearcher.Services;

/// <summary>
/// Windows 자동 시작 관리 서비스 인터페이스
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// 현재 자동 시작 등록 상태
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// 자동 시작 등록
    /// </summary>
    /// <param name="startMinimized">시작 시 최소화 여부</param>
    void Register(bool startMinimized = true);

    /// <summary>
    /// 자동 시작 해제
    /// </summary>
    void Unregister();

    /// <summary>
    /// 자동 시작 상태 확인
    /// </summary>
    bool CheckRegistration();
}
