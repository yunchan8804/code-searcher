namespace UnicodeSearcher.Plugins.Core;

/// <summary>
/// 플러그인 관리자 인터페이스
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// 등록된 모든 플러그인
    /// </summary>
    IReadOnlyList<IPlugin> Plugins { get; }

    /// <summary>
    /// 활성화된 플러그인만
    /// </summary>
    IReadOnlyList<IPlugin> EnabledPlugins { get; }

    /// <summary>
    /// 검색 가능한 활성화된 플러그인만
    /// </summary>
    IReadOnlyList<ISearchablePlugin> SearchablePlugins { get; }

    /// <summary>
    /// 현재 활성 플러그인
    /// </summary>
    ISearchablePlugin? ActivePlugin { get; }

    /// <summary>
    /// 활성 플러그인 변경
    /// </summary>
    void SetActivePlugin(string pluginId);

    /// <summary>
    /// 플러그인 등록
    /// </summary>
    void Register(IPlugin plugin);

    /// <summary>
    /// 플러그인 활성화/비활성화
    /// </summary>
    Task SetEnabledAsync(string pluginId, bool enabled);

    /// <summary>
    /// 플러그인 ID로 조회
    /// </summary>
    IPlugin? GetPlugin(string pluginId);

    /// <summary>
    /// 모든 플러그인 초기화
    /// </summary>
    Task InitializeAllAsync();

    /// <summary>
    /// 모든 플러그인 종료
    /// </summary>
    Task ShutdownAllAsync();

    /// <summary>
    /// 특정 플러그인 재초기화
    /// </summary>
    Task ReinitializePluginAsync(string pluginId);

    /// <summary>
    /// 플러그인 상태 변경 이벤트
    /// </summary>
    event EventHandler<PluginStateChangedEventArgs>? PluginStateChanged;

    /// <summary>
    /// 활성 플러그인 변경 이벤트
    /// </summary>
    event EventHandler<ActivePluginChangedEventArgs>? ActivePluginChanged;
}

/// <summary>
/// 플러그인 상태 변경 이벤트 인자
/// </summary>
public class PluginStateChangedEventArgs : EventArgs
{
    public required string PluginId { get; init; }
    public required bool IsEnabled { get; init; }
}

/// <summary>
/// 활성 플러그인 변경 이벤트 인자
/// </summary>
public class ActivePluginChangedEventArgs : EventArgs
{
    public ISearchablePlugin? PreviousPlugin { get; init; }
    public ISearchablePlugin? CurrentPlugin { get; init; }
}
