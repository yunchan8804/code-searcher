using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnicodeSearcher.Models;
using UnicodeSearcher.Plugins.Core;
using UnicodeSearcher.Services;

namespace UnicodeSearcher.ViewModels;

/// <summary>
/// 메인 창 ViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ICharacterDataService _characterDataService;
    private readonly ISearchService _searchService;
    private readonly IClipboardService _clipboardService;
    private readonly IRecentCharactersService _recentCharactersService;
    private readonly ISettingsService _settingsService;
    private readonly IFavoriteService _favoriteService;
    private readonly IPluginManager? _pluginManager;

    private CancellationTokenSource? _searchCts;
    private IReadOnlyList<UnicodeCharacter> _allCharacters = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<UnicodeCharacter> _filteredCharacters = [];

    [ObservableProperty]
    private UnicodeCharacter? _selectedCharacter;

    [ObservableProperty]
    private int _selectedIndex = -1;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _recentCharacters = [];

    [ObservableProperty]
    private ObservableCollection<string> _favoriteCharacters = [];

    [ObservableProperty]
    private ObservableCollection<QuickAccessItem> _recentItems = [];

    [ObservableProperty]
    private ObservableCollection<QuickAccessItem> _favoriteItems = [];

    [ObservableProperty]
    private bool _isGifMode = false;

    [ObservableProperty]
    private ObservableCollection<ISearchResult> _gifResults = [];

    [ObservableProperty]
    private ISearchResult? _selectedGifResult;

    [ObservableProperty]
    private bool _isGifLoading = false;

    [ObservableProperty]
    private string? _selectedGifTitle;

    [ObservableProperty]
    private string? _selectedGifDescription;

    [ObservableProperty]
    private bool _isDownloadingGif = false;

    [ObservableProperty]
    private string _downloadingMessage = "GIF 다운로드 중...";

    /// <summary>
    /// 현재 필터링된 문자 수
    /// </summary>
    public int FilteredCount => IsGifMode ? GifResults.Count : FilteredCharacters.Count;

    /// <summary>
    /// 전체 문자 수
    /// </summary>
    public int TotalCount => _allCharacters.Count;

    /// <summary>
    /// 설정
    /// </summary>
    public UserSettings Settings => _settingsService.Settings;

    /// <summary>
    /// 창 닫기 요청 이벤트
    /// </summary>
    public event EventHandler? CloseWindowRequested;

    /// <summary>
    /// 문자가 복사되었을 때 발생
    /// </summary>
    public event EventHandler<string>? CharacterCopied;

    public MainViewModel(
        ICharacterDataService characterDataService,
        ISearchService searchService,
        IClipboardService clipboardService,
        IRecentCharactersService recentCharactersService,
        ISettingsService settingsService,
        IFavoriteService favoriteService,
        IPluginManager? pluginManager = null)
    {
        _characterDataService = characterDataService;
        _searchService = searchService;
        _clipboardService = clipboardService;
        _recentCharactersService = recentCharactersService;
        _settingsService = settingsService;
        _favoriteService = favoriteService;
        _pluginManager = pluginManager;

        // 최근 사용 문자 변경 이벤트 구독
        _recentCharactersService.RecentCharactersChanged += (_, _) => UpdateRecentCharacters();

        // 즐겨찾기 변경 이벤트 구독
        _favoriteService.FavoritesChanged += (_, _) => UpdateFavoriteCharacters();
    }

    /// <summary>
    /// GIF 플러그인 사용 가능 여부
    /// </summary>
    public bool IsGifPluginAvailable =>
        _pluginManager?.SearchablePlugins.Any(p => p.Id == "gif") == true;

    /// <summary>
    /// 플러그인 상태 새로고침 (설정 변경 후 호출)
    /// </summary>
    public void RefreshPluginState()
    {
        OnPropertyChanged(nameof(IsGifPluginAvailable));
    }

    /// <summary>
    /// 데이터 초기화
    /// </summary>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "데이터 로딩 중...";

        try
        {
            // 설정 로드
            await _settingsService.LoadAsync();

            // 최근 사용 문자 로드
            await _recentCharactersService.LoadAsync();
            UpdateRecentCharacters();

            // 즐겨찾기 로드
            await _favoriteService.LoadAsync();
            UpdateFavoriteCharacters();

            // 문자 데이터 로드
            await _characterDataService.LoadDataAsync();

            _allCharacters = _characterDataService.Characters;

            // 카테고리 설정 (데이터 파일에서 로드)
            Categories = new ObservableCollection<Category>(_characterDataService.Categories);

            // 첫 번째 카테고리 ("전체") 선택
            SelectedCategory = Categories.FirstOrDefault();

            // 초기 문자 목록 표시
            UpdateFilteredCharacters();

            StatusMessage = $"{TotalCount}개 문자 로드 완료";
        }
        catch (Exception ex)
        {
            StatusMessage = $"데이터 로드 실패: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateRecentCharacters()
    {
        // 레거시 호환
        RecentCharacters = new ObservableCollection<string>(_recentCharactersService.RecentCharacters);

        // 통합 목록 (플러그인 활성화 여부에 따라 필터링)
        var items = _recentCharactersService.RecentItems
            .Where(i => IsPluginEnabled(i.PluginId))
            .ToList();
        RecentItems = new ObservableCollection<QuickAccessItem>(items);
    }

    private void UpdateFavoriteCharacters()
    {
        // 레거시 호환
        FavoriteCharacters = new ObservableCollection<string>(_favoriteService.Favorites);

        // 통합 목록 (플러그인 활성화 여부에 따라 필터링)
        var items = _favoriteService.FavoriteItems
            .Where(i => IsPluginEnabled(i.PluginId))
            .ToList();
        FavoriteItems = new ObservableCollection<QuickAccessItem>(items);
    }

    private bool IsPluginEnabled(string pluginId)
    {
        if (pluginId == "unicode") return true; // 유니코드는 항상 활성화
        if (pluginId == "gif") return IsGifPluginAvailable;
        return _pluginManager?.GetPlugin(pluginId)?.IsEnabled == true;
    }

    partial void OnSearchQueryChanged(string value)
    {
        // 검색어 변경 시 debounce 적용하여 검색
        if (IsGifMode)
        {
            SearchGifWithDebounce();
        }
        else
        {
            SearchWithDebounce();
        }
    }

    partial void OnIsGifModeChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredCount));
        if (value)
        {
            // GIF 모드로 전환 시 검색 수행
            SearchGifWithDebounce();
        }
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        // 카테고리 변경 시 검색 결과 업데이트
        UpdateFilteredCharacters();
    }

    partial void OnSelectedCharacterChanged(UnicodeCharacter? value)
    {
        if (value != null && !IsGifMode)
        {
            StatusMessage = $"{value.Char} {value.Name} ({value.Codepoint})";
        }
    }

    partial void OnSelectedGifResultChanged(ISearchResult? value)
    {
        if (value != null && IsGifMode)
        {
            SelectedGifTitle = value.Title;
            SelectedGifDescription = value.Description ?? "GIF";
        }
        else
        {
            SelectedGifTitle = null;
            SelectedGifDescription = null;
        }
    }

    partial void OnSelectedIndexChanged(int value)
    {
        if (value >= 0 && value < FilteredCharacters.Count)
        {
            SelectedCharacter = FilteredCharacters[value];
        }
    }

    private void SearchWithDebounce()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        _ = SearchAfterDelayAsync(_searchCts.Token);
    }

    private async Task SearchAfterDelayAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(150, ct); // 150ms debounce
            if (ct.IsCancellationRequested) return;

            UpdateFilteredCharacters();
        }
        catch (TaskCanceledException)
        {
            // 취소됨 - 무시
        }
    }

    private void UpdateFilteredCharacters()
    {
        var categoryCharacters = SelectedCategory?.Id == "all"
            ? _allCharacters
            : _characterDataService.GetCharactersByCategory(SelectedCategory?.Id ?? "all");

        var results = _searchService.Search(SearchQuery, categoryCharacters);

        FilteredCharacters = new ObservableCollection<UnicodeCharacter>(results);

        OnPropertyChanged(nameof(FilteredCount));

        // 첫 번째 결과 자동 선택
        if (FilteredCharacters.Count > 0)
        {
            SelectedIndex = 0;
        }
        else
        {
            SelectedIndex = -1;
            SelectedCharacter = null;
        }
    }

    /// <summary>
    /// 선택된 문자를 클립보드에 복사
    /// </summary>
    [RelayCommand]
    private void CopySelectedCharacter()
    {
        if (SelectedCharacter == null) return;

        CopyCharacterInternal(SelectedCharacter.Char, closeWindow: false);
    }

    /// <summary>
    /// 선택된 문자를 복사하고 창 닫기
    /// </summary>
    [RelayCommand]
    private void CopyAndClose()
    {
        if (SelectedCharacter == null) return;

        CopyCharacterInternal(SelectedCharacter.Char, closeWindow: true);
    }

    /// <summary>
    /// 선택된 문자를 복사하고 이전 창에 붙여넣기
    /// </summary>
    public event EventHandler? PasteRequested;

    [RelayCommand]
    private void PasteAndClose()
    {
        if (SelectedCharacter == null) return;

        try
        {
            // WinForms Clipboard 사용 - 블로킹 없이 빠르게 실패
            System.Windows.Forms.Clipboard.SetText(SelectedCharacter.Char);
        }
        catch
        {
            // 실패해도 계속 진행 (이미 클립보드에 있을 수 있음)
        }

        PasteRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 특정 문자를 클립보드에 복사
    /// </summary>
    [RelayCommand]
    private void CopyCharacter(UnicodeCharacter character)
    {
        CopyCharacterInternal(character.Char, closeWindow: Settings.Behavior.CloseOnSelect);
    }

    /// <summary>
    /// 최근 사용 문자 복사
    /// </summary>
    [RelayCommand]
    private void CopyRecentCharacter(string character)
    {
        CopyCharacterInternal(character, closeWindow: true);
    }

    /// <summary>
    /// QuickAccessItem 복사 (유니코드/GIF 통합)
    /// </summary>
    [RelayCommand]
    private async Task CopyQuickAccessItemAsync(QuickAccessItem item)
    {
        if (item == null) return;

        if (item.Type == QuickAccessItemType.Unicode)
        {
            // 유니코드 문자 복사
            CopyCharacterInternal(item.Value, closeWindow: true);
        }
        else if (item.Type == QuickAccessItemType.Gif)
        {
            // GIF 복사
            await CopyGifFromQuickAccessAsync(item);
        }
    }

    private async Task CopyGifFromQuickAccessAsync(QuickAccessItem item)
    {
        if (string.IsNullOrEmpty(item.FullUrl) && string.IsNullOrEmpty(item.PreviewUrl))
        {
            StatusMessage = "GIF URL이 없습니다.";
            return;
        }

        try
        {
            DownloadingMessage = "GIF 다운로드 중...";
            IsDownloadingGif = true;
            StatusMessage = "GIF 다운로드 중...";
            var gifUrl = item.FullUrl ?? item.PreviewUrl;

            // Tenor API 클라이언트 가져오기
            var gifPlugin = _pluginManager?.SearchablePlugins.FirstOrDefault(p => p.Id == "gif");
            if (gifPlugin == null)
            {
                // API 클라이언트 없이 URL만 복사
                System.Windows.Clipboard.SetText(gifUrl!);
                StatusMessage = "GIF URL 복사됨";
                PasteRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            // GIF 다운로드 (리플렉션으로 ApiClient 접근)
            var apiClientProp = gifPlugin.GetType().GetProperty("ApiClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (apiClientProp?.GetValue(gifPlugin) is Plugins.Gif.TenorApiClient apiClient)
            {
                var gifData = await apiClient.DownloadGifAsync(gifUrl!);
                if (gifData != null)
                {
                    var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"tenor_{item.Value}.gif");
                    await System.IO.File.WriteAllBytesAsync(tempPath, gifData);

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var dataObject = new System.Windows.DataObject();
                        var files = new System.Collections.Specialized.StringCollection { tempPath };
                        dataObject.SetFileDropList(files);
                        dataObject.SetText(gifUrl);
                        System.Windows.Clipboard.SetDataObject(dataObject, true);
                    });

                    StatusMessage = "GIF가 클립보드에 복사되었습니다!";
                    PasteRequested?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            // 다운로드 실패 시 URL만 복사
            System.Windows.Clipboard.SetText(gifUrl!);
            StatusMessage = "GIF URL 복사됨";
            PasteRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"GIF 복사 실패: {ex.Message}";
        }
        finally
        {
            IsDownloadingGif = false;
        }
    }

    /// <summary>
    /// 선택된 GIF를 클립보드에 복사
    /// </summary>
    [RelayCommand]
    private async Task CopySelectedGifAsync()
    {
        await CopyGifInternalAsync(autoPaste: false);
    }

    /// <summary>
    /// 선택된 GIF를 복사하고 이전 창에 붙여넣기
    /// </summary>
    [RelayCommand]
    private async Task PasteGifAndCloseAsync()
    {
        await CopyGifInternalAsync(autoPaste: true);
    }

    private async Task CopyGifInternalAsync(bool autoPaste)
    {
        if (SelectedGifResult == null) return;

        try
        {
            DownloadingMessage = "GIF 다운로드 중...";
            IsDownloadingGif = true;
            StatusMessage = "GIF 다운로드 중...";

            var content = await SelectedGifResult.GetClipboardContentAsync();

            // UI 스레드에서 클립보드 작업 실행
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var dataObject = new System.Windows.DataObject();

                    if (content.FilePaths?.Count > 0)
                    {
                        // 파일로 복사 (Discord, Slack 등에서 바로 붙여넣기 가능)
                        var files = new System.Collections.Specialized.StringCollection();
                        files.AddRange(content.FilePaths.ToArray());
                        dataObject.SetFileDropList(files);
                    }

                    if (!string.IsNullOrEmpty(content.Text))
                    {
                        dataObject.SetText(content.Text);
                    }

                    System.Windows.Clipboard.SetDataObject(dataObject, true);
                    StatusMessage = "GIF가 클립보드에 복사되었습니다!";
                }
                catch (Exception clipEx)
                {
                    StatusMessage = $"클립보드 오류: {clipEx.Message}";
                }
            });

            CharacterCopied?.Invoke(this, content.Text ?? "GIF");

            // 최근 사용에 GIF 추가
            if (SelectedGifResult is Plugins.Gif.GifSearchResult gifResult)
            {
                var recentItem = QuickAccessItem.FromGif(
                    gifResult.Id,
                    gifResult.Title,
                    gifResult.PreviewUrl,
                    gifResult.FullUrl);
                _recentCharactersService.AddItem(recentItem);
                await _recentCharactersService.SaveAsync();
            }

            // 자동 붙여넣기 또는 창 닫기
            if (autoPaste)
            {
                PasteRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (Settings.Behavior.CloseOnSelect)
            {
                CloseWindowRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"GIF 복사 실패: {ex.Message}";
        }
        finally
        {
            IsDownloadingGif = false;
        }
    }

    /// <summary>
    /// 인덱스로 최근 사용 문자 복사 (1-9)
    /// </summary>
    public void CopyRecentByIndex(int index)
    {
        if (index >= 0 && index < RecentCharacters.Count)
        {
            CopyCharacterInternal(RecentCharacters[index], closeWindow: true);
        }
    }

    /// <summary>
    /// 선택된 아이템 즐겨찾기 토글 (모드에 따라 유니코드/GIF)
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
        // GIF 모드인 경우 GIF 즐겨찾기
        if (IsGifMode)
        {
            ToggleGifFavorite();
            return;
        }

        // 유니코드 모드
        if (SelectedCharacter == null) return;

        _favoriteService.ToggleFavorite(SelectedCharacter.Char);
        _ = _favoriteService.SaveAsync();

        var isFavorite = _favoriteService.IsFavorite(SelectedCharacter.Char);
        StatusMessage = isFavorite
            ? $"'{SelectedCharacter.Char}' 즐겨찾기에 추가됨"
            : $"'{SelectedCharacter.Char}' 즐겨찾기에서 제거됨";
    }

    /// <summary>
    /// 특정 문자 즐겨찾기 토글
    /// </summary>
    [RelayCommand]
    private void ToggleFavoriteCharacter(UnicodeCharacter character)
    {
        _favoriteService.ToggleFavorite(character.Char);
        _ = _favoriteService.SaveAsync();

        var isFavorite = _favoriteService.IsFavorite(character.Char);
        StatusMessage = isFavorite
            ? $"'{character.Char}' 즐겨찾기에 추가됨"
            : $"'{character.Char}' 즐겨찾기에서 제거됨";
    }

    /// <summary>
    /// 문자가 즐겨찾기인지 확인
    /// </summary>
    public bool IsFavorite(string character) => _favoriteService.IsFavorite(character);

    /// <summary>
    /// 선택된 GIF 즐겨찾기 토글
    /// </summary>
    [RelayCommand]
    private void ToggleGifFavorite()
    {
        if (SelectedGifResult is not Plugins.Gif.GifSearchResult gifResult) return;

        var item = QuickAccessItem.FromGif(
            gifResult.Id,
            gifResult.Title,
            gifResult.PreviewUrl,
            gifResult.FullUrl);

        _favoriteService.ToggleFavoriteItem(item);
        _ = _favoriteService.SaveAsync();

        var isFavorite = _favoriteService.IsFavoriteItem(item);
        StatusMessage = isFavorite
            ? "GIF 즐겨찾기에 추가됨"
            : "GIF 즐겨찾기에서 제거됨";
    }

    /// <summary>
    /// GIF가 즐겨찾기인지 확인
    /// </summary>
    public bool IsGifFavorite(string gifId)
    {
        var item = QuickAccessItem.FromGif(gifId, null, null, null);
        return _favoriteService.IsFavoriteItem(item);
    }

    private void CopyCharacterInternal(string character, bool closeWindow)
    {
        if (_clipboardService.Copy(character))
        {
            StatusMessage = $"'{character}' 복사됨";

            // 최근 사용에 추가
            _recentCharactersService.AddCharacter(character);
            _ = _recentCharactersService.SaveAsync();

            CharacterCopied?.Invoke(this, character);

            if (closeWindow)
            {
                CloseWindowRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            StatusMessage = "복사 실패";
        }
    }

    /// <summary>
    /// 첫 번째 검색 결과 복사
    /// </summary>
    [RelayCommand]
    private void CopyFirstResult()
    {
        if (FilteredCharacters.Count > 0)
        {
            CopyCharacterInternal(FilteredCharacters[0].Char, closeWindow: true);
        }
    }

    /// <summary>
    /// 그리드에서 선택 이동
    /// </summary>
    public void MoveSelection(int deltaX, int deltaY, int columnsPerRow)
    {
        if (FilteredCharacters.Count == 0) return;

        var currentIndex = SelectedIndex >= 0 ? SelectedIndex : 0;
        var newIndex = currentIndex;

        if (deltaX != 0)
        {
            newIndex += deltaX;
        }

        if (deltaY != 0)
        {
            newIndex += deltaY * columnsPerRow;
        }

        // 범위 제한
        newIndex = Math.Max(0, Math.Min(newIndex, FilteredCharacters.Count - 1));

        SelectedIndex = newIndex;
    }

    /// <summary>
    /// 첫 번째 문자로 이동
    /// </summary>
    public void MoveToFirst()
    {
        if (FilteredCharacters.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    /// <summary>
    /// 마지막 문자로 이동
    /// </summary>
    public void MoveToLast()
    {
        if (FilteredCharacters.Count > 0)
        {
            SelectedIndex = FilteredCharacters.Count - 1;
        }
    }

    /// <summary>
    /// 다음 카테고리로 이동
    /// </summary>
    [RelayCommand]
    private void NextCategory()
    {
        if (Categories.Count == 0) return;

        var currentIndex = SelectedCategory != null
            ? Categories.IndexOf(SelectedCategory)
            : -1;

        var nextIndex = (currentIndex + 1) % Categories.Count;
        SelectedCategory = Categories[nextIndex];
    }

    /// <summary>
    /// 이전 카테고리로 이동
    /// </summary>
    [RelayCommand]
    private void PreviousCategory()
    {
        if (Categories.Count == 0) return;

        var currentIndex = SelectedCategory != null
            ? Categories.IndexOf(SelectedCategory)
            : 0;

        var prevIndex = currentIndex <= 0 ? Categories.Count - 1 : currentIndex - 1;
        SelectedCategory = Categories[prevIndex];
    }

    /// <summary>
    /// 검색어 초기화
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
    }

    /// <summary>
    /// 창이 닫힐 때 호출
    /// </summary>
    public void OnWindowClosing()
    {
        if (Settings.Behavior.ClearSearchOnClose)
        {
            SearchQuery = string.Empty;
        }
    }

    /// <summary>
    /// 앱 종료 시 저장
    /// </summary>
    public async Task SaveStateAsync()
    {
        await _settingsService.SaveAsync();
        await _recentCharactersService.SaveAsync();
        await _favoriteService.SaveAsync();
    }

    #region GIF 플러그인

    /// <summary>
    /// 플러그인 모드 토글 (유니코드 ↔ GIF)
    /// </summary>
    [RelayCommand]
    private void TogglePluginMode()
    {
        IsGifMode = !IsGifMode;
        StatusMessage = IsGifMode ? "GIF 검색 모드" : "유니코드 검색 모드";
    }

    private void SearchGifWithDebounce()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        _ = SearchGifAfterDelayAsync(_searchCts.Token);
    }

    private async Task SearchGifAfterDelayAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(500, ct); // GIF는 API 호출이므로 debounce 길게
            if (ct.IsCancellationRequested) return;

            await SearchGifAsync(ct);
        }
        catch (TaskCanceledException)
        {
            // 취소됨 - 무시
        }
    }

    private async Task SearchGifAsync(CancellationToken ct)
    {
        if (_pluginManager == null) return;

        var gifPlugin = _pluginManager.SearchablePlugins.FirstOrDefault(p => p.Id == "gif");
        if (gifPlugin == null) return;

        IsGifLoading = true;
        StatusMessage = "GIF 검색 중...";

        try
        {
            var results = await gifPlugin.SearchAsync(SearchQuery, ct);
            GifResults = new ObservableCollection<ISearchResult>(results);
            OnPropertyChanged(nameof(FilteredCount));

            StatusMessage = $"{GifResults.Count}개 GIF 검색됨";

            if (GifResults.Count > 0)
            {
                SelectedGifResult = GifResults[0];
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"GIF 검색 실패: {ex.Message}";
            GifResults.Clear();
        }
        finally
        {
            IsGifLoading = false;
        }
    }

    /// <summary>
    /// GIF 선택 및 복사
    /// </summary>
    [RelayCommand]
    private async Task CopyGifAsync()
    {
        if (SelectedGifResult == null) return;

        StatusMessage = "GIF 복사 중...";

        try
        {
            var content = await SelectedGifResult.GetClipboardContentAsync();

            if (content.FilePaths?.Count > 0)
            {
                // 파일로 클립보드에 복사
                var files = new System.Collections.Specialized.StringCollection();
                files.AddRange(content.FilePaths.ToArray());
                System.Windows.Clipboard.SetFileDropList(files);
                StatusMessage = "GIF 복사됨 (파일)";
            }
            else if (!string.IsNullOrEmpty(content.Text))
            {
                // URL로 복사
                System.Windows.Clipboard.SetText(content.Text);
                StatusMessage = "GIF URL 복사됨";
            }

            CloseWindowRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            StatusMessage = $"GIF 복사 실패: {ex.Message}";
        }
    }

    #endregion
}
