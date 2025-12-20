using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnicodeSearcher.Models;
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

    /// <summary>
    /// 현재 필터링된 문자 수
    /// </summary>
    public int FilteredCount => FilteredCharacters.Count;

    /// <summary>
    /// 전체 문자 수
    /// </summary>
    public int TotalCount => _allCharacters.Count;

    /// <summary>
    /// 즐겨찾기 문자 목록
    /// </summary>
    public IReadOnlySet<string> FavoriteCharacters => _favoriteService.Favorites;

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
        IFavoriteService favoriteService)
    {
        _characterDataService = characterDataService;
        _searchService = searchService;
        _clipboardService = clipboardService;
        _recentCharactersService = recentCharactersService;
        _settingsService = settingsService;
        _favoriteService = favoriteService;

        // 최근 사용 문자 변경 이벤트 구독
        _recentCharactersService.RecentCharactersChanged += (_, _) => UpdateRecentCharacters();

        // 즐겨찾기 변경 이벤트 구독
        _favoriteService.FavoritesChanged += (_, _) => OnPropertyChanged(nameof(FavoriteCharacters));
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

            // 문자 데이터 로드
            await _characterDataService.LoadDataAsync();

            _allCharacters = _characterDataService.Characters;

            // 카테고리 설정 ("전체" 추가)
            var allCategory = new Category
            {
                Id = "all",
                NameKo = "전체",
                NameEn = "All",
                Icon = "🔤",
                Order = 0
            };

            Categories = new ObservableCollection<Category>(
                new[] { allCategory }.Concat(_characterDataService.Categories)
            );

            SelectedCategory = allCategory;

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
        RecentCharacters = new ObservableCollection<string>(_recentCharactersService.RecentCharacters);
    }

    partial void OnSearchQueryChanged(string value)
    {
        // 검색어 변경 시 debounce 적용하여 검색
        SearchWithDebounce();
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        // 카테고리 변경 시 검색 결과 업데이트
        UpdateFilteredCharacters();
    }

    partial void OnSelectedCharacterChanged(UnicodeCharacter? value)
    {
        if (value != null)
        {
            StatusMessage = $"{value.Char} {value.Name} ({value.Codepoint})";
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
        Helpers.DebugLogger.Log("PasteAndClose START");

        if (SelectedCharacter == null) return;

        Helpers.DebugLogger.Log("Before Clipboard (WinForms)");
        try
        {
            // WinForms Clipboard 사용 - 블로킹 없이 빠르게 실패
            System.Windows.Forms.Clipboard.SetText(SelectedCharacter.Char);
        }
        catch (Exception ex)
        {
            Helpers.DebugLogger.Log($"Clipboard ERROR: {ex.Message}");
            // 실패해도 계속 진행 (이미 클립보드에 있을 수 있음)
        }
        Helpers.DebugLogger.Log("After Clipboard");

        Helpers.DebugLogger.Log("Before PasteRequested.Invoke");
        PasteRequested?.Invoke(this, EventArgs.Empty);
        Helpers.DebugLogger.Log("After PasteRequested.Invoke");

        Helpers.DebugLogger.Log("PasteAndClose END");
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
    /// 선택된 문자 즐겨찾기 토글
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite()
    {
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
}
