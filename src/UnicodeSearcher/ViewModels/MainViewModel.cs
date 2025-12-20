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

    /// <summary>
    /// 현재 필터링된 문자 수
    /// </summary>
    public int FilteredCount => FilteredCharacters.Count;

    /// <summary>
    /// 전체 문자 수
    /// </summary>
    public int TotalCount => _allCharacters.Count;

    public MainViewModel(
        ICharacterDataService characterDataService,
        ISearchService searchService,
        IClipboardService clipboardService)
    {
        _characterDataService = characterDataService;
        _searchService = searchService;
        _clipboardService = clipboardService;
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

        CopyCharacter(SelectedCharacter);
    }

    /// <summary>
    /// 특정 문자를 클립보드에 복사
    /// </summary>
    [RelayCommand]
    private void CopyCharacter(UnicodeCharacter character)
    {
        if (_clipboardService.Copy(character.Char))
        {
            StatusMessage = $"'{character.Char}' 복사됨";
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
            CopyCharacter(FilteredCharacters[0]);
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
}
