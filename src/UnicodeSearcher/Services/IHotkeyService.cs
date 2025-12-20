using System.Windows.Input;

namespace UnicodeSearcher.Services;

/// <summary>
/// 글로벌 핫키 서비스 인터페이스
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// 핫키가 눌렸을 때 발생하는 이벤트
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// 핫키 등록
    /// </summary>
    bool RegisterHotkey(ModifierKeys modifiers, Key key);

    /// <summary>
    /// 핫키 해제
    /// </summary>
    void UnregisterHotkey();

    /// <summary>
    /// 핫키가 등록되어 있는지 여부
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// 핫키 시작
    /// </summary>
    void Start();

    /// <summary>
    /// 핫키 중지
    /// </summary>
    void Stop();
}
