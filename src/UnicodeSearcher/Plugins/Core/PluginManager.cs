using System.Diagnostics;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.Plugins.Core;

/// <summary>
/// 플러그인 관리자 구현
/// </summary>
public class PluginManager : IPluginManager
{
    private readonly ISettingsService _settingsService;
    private readonly List<IPlugin> _plugins = [];
    private ISearchablePlugin? _activePlugin;

    public PluginManager(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> Plugins => _plugins.OrderBy(p => p.Order).ToList();

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> EnabledPlugins =>
        _plugins.Where(p => p.IsEnabled).OrderBy(p => p.Order).ToList();

    /// <inheritdoc/>
    public IReadOnlyList<ISearchablePlugin> SearchablePlugins =>
        _plugins
            .OfType<ISearchablePlugin>()
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.Order)
            .ToList();

    /// <inheritdoc/>
    public ISearchablePlugin? ActivePlugin => _activePlugin;

    /// <inheritdoc/>
    public event EventHandler<PluginStateChangedEventArgs>? PluginStateChanged;

    /// <inheritdoc/>
    public event EventHandler<ActivePluginChangedEventArgs>? ActivePluginChanged;

    /// <inheritdoc/>
    public void Register(IPlugin plugin)
    {
        if (_plugins.Any(p => p.Id == plugin.Id))
        {
            Debug.WriteLine($"[PluginManager] Plugin '{plugin.Id}' already registered, skipping");
            return;
        }

        // 설정에서 활성화 상태 로드
        var settings = _settingsService.Settings.Plugins;
        if (settings.Enabled.TryGetValue(plugin.Id, out var enabled))
        {
            plugin.IsEnabled = enabled;
        }
        else
        {
            // 기본값 설정 (unicode는 기본 활성화)
            plugin.IsEnabled = plugin.Id == "unicode";
            settings.Enabled[plugin.Id] = plugin.IsEnabled;
        }

        _plugins.Add(plugin);
        Debug.WriteLine($"[PluginManager] Registered plugin: {plugin.Id} (enabled: {plugin.IsEnabled})");
    }

    /// <inheritdoc/>
    public void SetActivePlugin(string pluginId)
    {
        var plugin = SearchablePlugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin == null)
        {
            Debug.WriteLine($"[PluginManager] Plugin '{pluginId}' not found or not enabled");
            return;
        }

        if (_activePlugin?.Id == pluginId)
            return;

        var previous = _activePlugin;
        _activePlugin = plugin;

        ActivePluginChanged?.Invoke(this, new ActivePluginChangedEventArgs
        {
            PreviousPlugin = previous,
            CurrentPlugin = plugin
        });

        Debug.WriteLine($"[PluginManager] Active plugin changed: {previous?.Id ?? "none"} -> {pluginId}");
    }

    /// <inheritdoc/>
    public async Task SetEnabledAsync(string pluginId, bool enabled)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin == null)
        {
            Debug.WriteLine($"[PluginManager] Plugin '{pluginId}' not found");
            return;
        }

        if (plugin.IsEnabled == enabled)
            return;

        plugin.IsEnabled = enabled;

        // 설정 저장
        _settingsService.Settings.Plugins.Enabled[pluginId] = enabled;
        await _settingsService.SaveAsync();

        // 활성 플러그인이 비활성화되면 다른 플러그인으로 전환
        if (!enabled && _activePlugin?.Id == pluginId)
        {
            var nextPlugin = SearchablePlugins.FirstOrDefault();
            if (nextPlugin != null)
            {
                SetActivePlugin(nextPlugin.Id);
            }
            else
            {
                var previous = _activePlugin;
                _activePlugin = null;
                ActivePluginChanged?.Invoke(this, new ActivePluginChangedEventArgs
                {
                    PreviousPlugin = previous,
                    CurrentPlugin = null
                });
            }
        }

        PluginStateChanged?.Invoke(this, new PluginStateChangedEventArgs
        {
            PluginId = pluginId,
            IsEnabled = enabled
        });

        Debug.WriteLine($"[PluginManager] Plugin '{pluginId}' enabled: {enabled}");
    }

    /// <inheritdoc/>
    public IPlugin? GetPlugin(string pluginId) =>
        _plugins.FirstOrDefault(p => p.Id == pluginId);

    /// <inheritdoc/>
    public async Task InitializeAllAsync()
    {
        Debug.WriteLine($"[PluginManager] Initializing {_plugins.Count} plugins...");

        foreach (var plugin in _plugins)
        {
            try
            {
                await plugin.InitializeAsync();
                Debug.WriteLine($"[PluginManager] Initialized: {plugin.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PluginManager] Failed to initialize {plugin.Id}: {ex.Message}");
                plugin.IsEnabled = false;
            }
        }

        // 첫 번째 활성화된 검색 플러그인을 기본 활성 플러그인으로 설정
        var firstSearchable = SearchablePlugins.FirstOrDefault();
        if (firstSearchable != null)
        {
            SetActivePlugin(firstSearchable.Id);
        }

        Debug.WriteLine("[PluginManager] All plugins initialized");
    }

    /// <inheritdoc/>
    public async Task ShutdownAllAsync()
    {
        Debug.WriteLine("[PluginManager] Shutting down all plugins...");

        foreach (var plugin in _plugins)
        {
            try
            {
                await plugin.ShutdownAsync();
                Debug.WriteLine($"[PluginManager] Shutdown: {plugin.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PluginManager] Failed to shutdown {plugin.Id}: {ex.Message}");
            }
        }

        Debug.WriteLine("[PluginManager] All plugins shut down");
    }

    /// <inheritdoc/>
    public async Task ReinitializePluginAsync(string pluginId)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Id == pluginId);
        if (plugin == null)
        {
            Debug.WriteLine($"[PluginManager] Plugin '{pluginId}' not found for reinitialization");
            return;
        }

        try
        {
            // 종료 후 재초기화
            await plugin.ShutdownAsync();
            await plugin.InitializeAsync();

            // 설정에서 활성화 상태 다시 로드
            var settings = _settingsService.Settings.Plugins;
            if (settings.Enabled.TryGetValue(pluginId, out var enabled))
            {
                plugin.IsEnabled = enabled;
            }

            Debug.WriteLine($"[PluginManager] Reinitialized plugin: {pluginId} (enabled: {plugin.IsEnabled})");

            // 상태 변경 이벤트 발생
            PluginStateChanged?.Invoke(this, new PluginStateChangedEventArgs
            {
                PluginId = pluginId,
                IsEnabled = plugin.IsEnabled
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginManager] Failed to reinitialize {pluginId}: {ex.Message}");
            plugin.IsEnabled = false;
        }
    }
}
