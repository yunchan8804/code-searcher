using System.Diagnostics;
using UnicodeSearcher.Plugins.Core;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.Plugins.Gif;

/// <summary>
/// GIF 검색 플러그인 (Tenor API)
/// </summary>
public class GifPlugin : ISearchablePlugin, IDisposable
{
    private readonly ISettingsService _settingsService;
    private TenorApiClient? _apiClient;
    private string? _apiKey;

    public GifPlugin(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    #region IPlugin

    /// <inheritdoc/>
    public string Id => "gif";

    /// <inheritdoc/>
    public string DisplayName => "GIF";

    /// <inheritdoc/>
    public string Description => "Tenor를 통한 GIF 검색";

    /// <inheritdoc/>
    public Version Version => new(1, 0, 0);

    /// <inheritdoc/>
    public string Icon => "🎬";

    /// <inheritdoc/>
    public bool IsEnabled { get; set; }

    /// <inheritdoc/>
    public int Order => 10;  // 유니코드(0) 다음

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        // 1. 설정에서 API 키 읽기
        _apiKey = _settingsService.Settings.Plugins.Gif.TenorApiKey;

        // 2. 설정에 없으면 환경 변수에서 시도 (User 레벨)
        if (string.IsNullOrEmpty(_apiKey))
        {
            _apiKey = Environment.GetEnvironmentVariable("TENOR_API_KEY", EnvironmentVariableTarget.User);
        }

        // 3. User 레벨에서 못 찾으면 Process 레벨에서 시도
        if (string.IsNullOrEmpty(_apiKey))
        {
            _apiKey = Environment.GetEnvironmentVariable("TENOR_API_KEY");
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            Debug.WriteLine("[GifPlugin] Tenor API key not found. Plugin disabled.");
            IsEnabled = false;
            return Task.CompletedTask;
        }

        _apiClient = new TenorApiClient(_apiKey);
        Debug.WriteLine("[GifPlugin] Initialized with API key");

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ShutdownAsync()
    {
        _apiClient?.Dispose();
        _apiClient = null;
        return Task.CompletedTask;
    }

    #endregion

    #region ISearchablePlugin

    /// <inheritdoc/>
    public string SearchPlaceholder => "GIF 검색... (예: happy, cat, thumbs up)";

    /// <inheritdoc/>
    public int SearchDebounceMs => 500;  // API 호출이므로 debounce 길게

    /// <inheritdoc/>
    public IReadOnlyList<PluginCategory>? Categories => null;  // 카테고리 없음

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (_apiClient == null)
        {
            Debug.WriteLine("[GifPlugin] API client not initialized");
            return [];
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            // 빈 검색어면 인기 GIF 반환
            var featured = await _apiClient.GetFeaturedAsync(20, cancellationToken);
            if (featured?.Results == null)
                return [];

            return featured.Results
                .Select(g => new GifSearchResult(g, _apiClient))
                .ToList();
        }

        var response = await _apiClient.SearchAsync(query, 20, null, cancellationToken);

        if (response?.Results == null)
            return [];

        return response.Results
            .Select(g => new GifSearchResult(g, _apiClient))
            .ToList();
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ISearchResult>> SearchByCategoryAsync(
        string categoryId,
        string query,
        CancellationToken cancellationToken = default)
    {
        // 카테고리 없음
        return SearchAsync(query, cancellationToken);
    }

    #endregion

    public void Dispose()
    {
        _apiClient?.Dispose();
    }
}
