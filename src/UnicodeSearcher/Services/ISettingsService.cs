using UnicodeSearcher.Models;

namespace UnicodeSearcher.Services;

/// <summary>
/// 설정 서비스 인터페이스
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 현재 설정
    /// </summary>
    UserSettings Settings { get; }

    /// <summary>
    /// 설정이 변경되었을 때 발생
    /// </summary>
    event EventHandler<UserSettings>? SettingsChanged;

    /// <summary>
    /// 설정 로드
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// 설정 저장
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// 설정 업데이트
    /// </summary>
    void UpdateSettings(Action<UserSettings> updateAction);
}
