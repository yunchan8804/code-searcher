using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using UnicodeSearcher.Helpers;
using UnicodeSearcher.Models;
using UnicodeSearcher.ViewModels;

namespace UnicodeSearcher;

/// <summary>
/// MainWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class MainWindow : Window
{
    #region Windows 11 둥근 모서리 API

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;
    private const int DWMWCP_ROUNDSMALL = 3;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    private void ApplyRoundedCorners()
    {
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
            }
        }
        catch
        {
            // Windows 10 이하에서는 무시 (WPF 기본 둥근 모서리 사용)
        }
    }

    #endregion

    private MainViewModel ViewModel => (MainViewModel)DataContext;

    // 그리드에서 한 행에 표시되는 문자 수 (대략적 계산용)
    private int ColumnsPerRow => (int)((CharacterGrid.ActualWidth - 16) / 54); // 48 + 6 margin

    // 포커스 잃을 때 창 숨기기 여부
    private bool _hideOnDeactivate = true;

    // 창 표시 직후 Deactivated 무시용 타이머
    private DateTime _lastShowTime = DateTime.MinValue;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Windows 11 네이티브 둥근 모서리 적용
        ApplyRoundedCorners();

        // 검색창에 포커스
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();
        HighlightSection("search");

        // ViewModel 이벤트 구독
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.CloseWindowRequested += OnCloseWindowRequested;
            viewModel.PasteRequested += OnPasteRequested;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        // 데이터 로드
        await ViewModel.InitializeAsync();

        // 첫 번째 카테고리 선택
        SelectFirstCategory();
    }

    private void OnCloseWindowRequested(object? sender, EventArgs e)
    {
        HideWindow();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedCategory))
        {
            ScrollCategoryIntoView();
        }
    }

    private void ScrollCategoryIntoView()
    {
        if (ViewModel.SelectedCategory == null) return;

        // 선택된 카테고리의 인덱스 찾기
        var index = ViewModel.Categories.IndexOf(ViewModel.SelectedCategory);
        if (index < 0) return;

        // ItemsControl에서 해당 컨테이너 찾기
        var container = CategoryTabs.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
        if (container != null)
        {
            // BringIntoView로 스크롤
            container.BringIntoView();
        }
    }

    private void OnPasteRequested(object? sender, EventArgs e)
    {
        HideWindow();

        // 완전히 백그라운드 스레드에서 실행
        Task.Run(async () =>
        {
            await Task.Delay(150);
            WindowHelper.PasteToActiveWindow();
        });
    }

    /// <summary>
    /// 창 표시
    /// </summary>
    public async void ShowWindow(bool positionNearCursor = true)
    {
        _hideOnDeactivate = true;
        _lastShowTime = DateTime.Now;

        if (positionNearCursor)
        {
            WindowHelper.PositionNearCursor(this);
        }

        // 창 표시 및 활성화
        Show();
        WindowState = WindowState.Normal;
        Topmost = true;

        // 핫키 이벤트가 완전히 처리될 때까지 대기 (KeyUp 포함)
        await Task.Delay(100);

        // Win32 API로 강제 포커스
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            ShowWindow(hwnd, SW_RESTORE);
            SetForegroundWindow(hwnd);
        }

        Activate();
        Focus();

        // 검색창 초기화 (핫키 문자 제거)
        ViewModel.SearchQuery = string.Empty;
        SearchTextBox.Focus();
        HighlightSection("search");
    }

    /// <summary>
    /// 창 숨기기
    /// </summary>
    public void HideWindow()
    {
        _hideOnDeactivate = false;
        Hide();

        // 창 숨긴 후 백그라운드에서 정리
        Dispatcher.BeginInvoke(() => ViewModel.OnWindowClosing(), DispatcherPriority.Background);
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // 창 표시 직후 500ms 동안은 Deactivated 무시
        if ((DateTime.Now - _lastShowTime).TotalMilliseconds < 500)
        {
            return;
        }

        // 포커스 잃으면 창 숨기기 (설정에 따라)
        if (_hideOnDeactivate && IsVisible)
        {
            HideWindow();
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // 창 닫기 대신 숨기기 (시스템 트레이에서 계속 실행)
        e.Cancel = true;
        HideWindow();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // ESC: 창 닫기
        if (e.Key == Key.Escape)
        {
            HideWindow();
            e.Handled = true;
            return;
        }

        // Ctrl + 방향키: 카테고리 이동, Ctrl+D: 즐겨찾기 토글, Ctrl+F: 검색창 포커스
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.Left)
            {
                ViewModel.PreviousCategoryCommand.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Right)
            {
                ViewModel.NextCategoryCommand.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.D)
            {
                // Ctrl+D: 즐겨찾기 토글
                ViewModel.ToggleFavoriteCommand.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.F)
            {
                // Ctrl+F: 검색창으로 포커스
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                HighlightSection("search");
                e.Handled = true;
                return;
            }
            if (e.Key == Key.OemComma)
            {
                // Ctrl+,: 설정 창 열기
                OpenSettings();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.D1)
            {
                // Ctrl+1: 유니코드 탭
                SwitchToPlugin("unicode");
                e.Handled = true;
                return;
            }
            if (e.Key == Key.D2)
            {
                // Ctrl+2: GIF 탭
                SwitchToPlugin("gif");
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Tab)
            {
                // Ctrl+Tab / Ctrl+Shift+Tab: 탭 순환
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    SwitchToPreviousPlugin();
                }
                else
                {
                    SwitchToNextPlugin();
                }
                e.Handled = true;
                return;
            }
        }

        // 숫자키: 최근 사용 문자 (검색어 비었을 때, Ctrl 없이)
        if (string.IsNullOrEmpty(ViewModel.SearchQuery))
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int index = e.Key - Key.D1;
                ViewModel.CopyRecentByIndex(index);
                e.Handled = true;
                return;
            }
        }

        // Ctrl+숫자키: N번째 검색 결과
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int index = e.Key - Key.D1;
                if (index < ViewModel.FilteredCharacters.Count)
                {
                    ViewModel.SelectedIndex = index;
                    ViewModel.CopyAndCloseCommand.Execute(null);
                }
                e.Handled = true;
                return;
            }
        }

        // 검색창에 포커스가 있을 때
        if (SearchTextBox.IsFocused)
        {
            HandleSearchBoxKeyDown(e);
            return;
        }

        // 최근 사용 문자 리스트에 포커스가 있을 때
        if (RecentList.IsKeyboardFocusWithin)
        {
            HandleQuickListKeyDown(RecentList, e);
            return;
        }

        // 즐겨찾기 리스트에 포커스가 있을 때
        if (FavoriteList.IsKeyboardFocusWithin)
        {
            HandleQuickListKeyDown(FavoriteList, e);
            return;
        }

        // 카테고리 탭에 포커스가 있을 때
        if (CategoryTabs.IsFocused)
        {
            HandleCategoryKeyDown(e);
            return;
        }

        // GIF 그리드에 포커스가 있을 때
        if (GifGrid.IsKeyboardFocusWithin && ViewModel.IsGifMode)
        {
            HandleGifGridKeyDown(e);
            return;
        }

        // 유니코드 그리드 탐색
        if (!ViewModel.IsGifMode)
        {
            HandleGridKeyDown(e);
        }
    }

    private void HandleGifGridKeyDown(KeyEventArgs e)
    {
        // GIF 그리드는 WrapPanel이라 열 수 계산이 다름
        var gridWidth = GifGrid.ActualWidth - 16; // padding 고려
        var itemWidth = 108; // 100 + margin 8
        var columns = Math.Max(1, (int)(gridWidth / itemWidth));

        var currentIndex = GifGrid.SelectedIndex;
        var itemCount = ViewModel.GifResults.Count;

        if (itemCount == 0) return;

        switch (e.Key)
        {
            case Key.Left:
                if (currentIndex > 0)
                {
                    GifGrid.SelectedIndex = currentIndex - 1;
                    GifGrid.ScrollIntoView(GifGrid.SelectedItem);
                }
                e.Handled = true;
                break;

            case Key.Right:
                if (currentIndex < itemCount - 1)
                {
                    GifGrid.SelectedIndex = currentIndex + 1;
                    GifGrid.ScrollIntoView(GifGrid.SelectedItem);
                }
                e.Handled = true;
                break;

            case Key.Up:
                if (currentIndex >= columns)
                {
                    GifGrid.SelectedIndex = currentIndex - columns;
                    GifGrid.ScrollIntoView(GifGrid.SelectedItem);
                }
                e.Handled = true;
                break;

            case Key.Down:
                if (currentIndex + columns < itemCount)
                {
                    GifGrid.SelectedIndex = currentIndex + columns;
                    GifGrid.ScrollIntoView(GifGrid.SelectedItem);
                }
                e.Handled = true;
                break;

            case Key.Enter:
                if (ViewModel.SelectedGifResult != null)
                {
                    ViewModel.PasteGifAndCloseCommand.Execute(null);
                }
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Shift+Tab: 검색창으로
                    SearchTextBox.Focus();
                }
                e.Handled = true;
                break;
        }
    }

    private void HandleSearchBoxKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                // 첫 번째 결과 붙여넣기 + 닫기
                if (ViewModel.FilteredCharacters.Count > 0)
                {
                    ViewModel.SelectedIndex = 0;
                    ViewModel.PasteAndCloseCommand.Execute(null);
                }
                e.Handled = true;
                break;

            case Key.Down:
                // 다음 영역으로 이동
                FocusNextArea("search");
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    FocusNextArea("search");
                    e.Handled = true;
                }
                break;
        }
    }

    /// <summary>
    /// 다음 영역으로 포커스 이동 (검색 → 최근 → 즐겨찾기 → 카테고리 → 그리드)
    /// </summary>
    private void FocusNextArea(string currentArea)
    {
        switch (currentArea)
        {
            case "search":
                if (RecentList.Items.Count > 0 && RecentList.IsVisible)
                {
                    RecentList.Focus();
                    if (RecentList.SelectedIndex < 0) RecentList.SelectedIndex = 0;
                    HighlightSection("recent");
                }
                else if (FavoriteList.Items.Count > 0 && FavoriteList.IsVisible)
                {
                    FavoriteList.Focus();
                    if (FavoriteList.SelectedIndex < 0) FavoriteList.SelectedIndex = 0;
                    HighlightSection("favorite");
                }
                else
                {
                    FocusOnCategory();
                }
                break;

            case "recent":
                if (FavoriteList.Items.Count > 0 && FavoriteList.IsVisible)
                {
                    FavoriteList.Focus();
                    if (FavoriteList.SelectedIndex < 0) FavoriteList.SelectedIndex = 0;
                    HighlightSection("favorite");
                }
                else
                {
                    FocusOnCategory();
                }
                break;

            case "favorite":
                FocusOnCategory();
                break;

            case "category":
                FocusOnGrid();
                break;
        }
    }

    /// <summary>
    /// 이전 영역으로 포커스 이동 (그리드 → 카테고리 → 즐겨찾기 → 최근 → 검색)
    /// </summary>
    private void FocusPreviousArea(string currentArea)
    {
        switch (currentArea)
        {
            case "grid":
                FocusOnCategory();
                break;

            case "category":
                if (FavoriteList.Items.Count > 0 && FavoriteList.IsVisible)
                {
                    FavoriteList.Focus();
                    if (FavoriteList.SelectedIndex < 0) FavoriteList.SelectedIndex = 0;
                    HighlightSection("favorite");
                }
                else if (RecentList.Items.Count > 0 && RecentList.IsVisible)
                {
                    RecentList.Focus();
                    if (RecentList.SelectedIndex < 0) RecentList.SelectedIndex = 0;
                    HighlightSection("recent");
                }
                else
                {
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
                    HighlightSection("search");
                }
                break;

            case "favorite":
                if (RecentList.Items.Count > 0 && RecentList.IsVisible)
                {
                    RecentList.Focus();
                    if (RecentList.SelectedIndex < 0) RecentList.SelectedIndex = 0;
                    HighlightSection("recent");
                }
                else
                {
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
                    HighlightSection("search");
                }
                break;

            case "recent":
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                HighlightSection("search");
                break;
        }
    }

    private void HandleCategoryKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                ViewModel.PreviousCategoryCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Right:
                ViewModel.NextCategoryCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Up:
                // 이전 영역으로 이동 (즐겨찾기 → 최근 → 검색)
                FocusPreviousArea("category");
                e.Handled = true;
                break;

            case Key.Down:
            case Key.Enter:
                // 그리드로 포커스 이동
                if (ViewModel.FilteredCharacters.Count > 0)
                {
                    ViewModel.SelectedIndex = 0;
                    FocusOnGrid();
                }
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    FocusPreviousArea("category");
                }
                else
                {
                    FocusNextArea("category");
                }
                e.Handled = true;
                break;
        }
    }

    private void CategoryTabs_KeyDown(object sender, KeyEventArgs e)
    {
        HandleCategoryKeyDown(e);
    }

    private void HandleGridKeyDown(KeyEventArgs e)
    {
        var columns = Math.Max(1, ColumnsPerRow);

        switch (e.Key)
        {
            case Key.Left:
                ViewModel.MoveSelection(-1, 0, columns);
                e.Handled = true;
                break;

            case Key.Right:
                ViewModel.MoveSelection(1, 0, columns);
                e.Handled = true;
                break;

            case Key.Up:
                if (ViewModel.SelectedIndex < columns)
                {
                    // 첫 행에서 위로 가면 카테고리 탭으로
                    FocusOnCategory();
                }
                else
                {
                    ViewModel.MoveSelection(0, -1, columns);
                }
                e.Handled = true;
                break;

            case Key.Down:
                ViewModel.MoveSelection(0, 1, columns);
                e.Handled = true;
                break;

            case Key.Enter:
                // 복사 + 이전 창에 붙여넣기 + 창 닫기
                ViewModel.PasteAndCloseCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.C:
                // Ctrl+C: 복사만 (창 유지)
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ViewModel.CopySelectedCharacterCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Space:
                // Space: 복사 (창 유지) - 여러 개 복사 시 유용
                ViewModel.CopySelectedCharacterCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Home:
                ViewModel.MoveToFirst();
                e.Handled = true;
                break;

            case Key.End:
                ViewModel.MoveToLast();
                e.Handled = true;
                break;

            case Key.PageUp:
                // 한 페이지 위로 (대략 5행)
                ViewModel.MoveSelection(0, -5, columns);
                e.Handled = true;
                break;

            case Key.PageDown:
                // 한 페이지 아래로 (대략 5행)
                ViewModel.MoveSelection(0, 5, columns);
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // 이전 영역으로 (카테고리)
                    FocusPreviousArea("grid");
                    e.Handled = true;
                }
                break;

            default:
                // 알파벳/한글 입력 시 검색창으로 포커스
                if (IsTypingKey(e.Key))
                {
                    SearchTextBox.Focus();
                    HighlightSection("search");
                    // 키 이벤트는 계속 전달되어 검색창에 입력됨
                }
                break;
        }
    }

    private static bool IsTypingKey(Key key)
    {
        // 알파벳, 숫자, 한글 등 입력 가능한 키 확인
        return (key >= Key.A && key <= Key.Z) ||
               (key >= Key.D0 && key <= Key.D9) ||
               (key >= Key.NumPad0 && key <= Key.NumPad9) ||
               key == Key.OemMinus ||
               key == Key.OemPlus ||
               key == Key.OemOpenBrackets ||
               key == Key.OemCloseBrackets ||
               key == Key.OemSemicolon ||
               key == Key.OemQuotes ||
               key == Key.OemComma ||
               key == Key.OemPeriod ||
               key == Key.OemQuestion ||
               key == Key.OemBackslash ||
               key == Key.ImeProcessed; // 한글 입력
    }

    private void FocusOnGrid()
    {
        // GIF 모드일 때는 GIF 그리드에 포커스
        if (ViewModel.IsGifMode)
        {
            GifGrid.Focus();
            if (GifGrid.SelectedIndex < 0 && ViewModel.GifResults.Count > 0)
            {
                GifGrid.SelectedIndex = 0;
            }
            HighlightSection("grid");
        }
        else
        {
            // 유니코드 그리드에 포커스
            CharacterGrid.Focus();
            HighlightSection("grid");
        }
    }

    private void FocusOnCategory()
    {
        // GIF 모드일 때는 카테고리가 없으므로 바로 그리드로
        if (ViewModel.IsGifMode)
        {
            FocusOnGrid();
            return;
        }

        // 카테고리 탭에 포커스
        CategoryTabs.Focus();
        HighlightSection("category");
    }

    /// <summary>
    /// 현재 포커스된 섹션 하이라이트
    /// </summary>
    private void HighlightSection(string sectionName)
    {
        // 모든 섹션 기본 배경으로 초기화
        SearchSection.Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush");
        RecentFavoriteSection.Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush");
        CategorySection.Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush");
        GridSection.Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush");

        // 현재 섹션 하이라이트 (배경색 더 진하게)
        switch (sectionName)
        {
            case "search":
                SearchSection.Background = (System.Windows.Media.Brush)FindResource("FocusedSectionBrush");
                break;
            case "recent":
            case "favorite":
                RecentFavoriteSection.Background = (System.Windows.Media.Brush)FindResource("FocusedSectionBrush");
                break;
            case "category":
                CategorySection.Background = (System.Windows.Media.Brush)FindResource("FocusedSectionBrush");
                break;
            case "grid":
                GridSection.Background = (System.Windows.Media.Brush)FindResource("FocusedSectionBrush");
                break;
        }
    }

    private void SelectFirstCategory()
    {
        // 첫 번째 카테고리 버튼 선택
        if (CategoryTabs.Items.Count > 0)
        {
            var container = CategoryTabs.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            if (container != null)
            {
                var radioButton = FindVisualChild<RadioButton>(container);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }
    }

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && radioButton.Tag is Category category)
        {
            ViewModel.SelectedCategory = category;
        }
    }

    private void RecentList_KeyDown(object sender, KeyEventArgs e)
    {
        HandleQuickListKeyDown(RecentList, e);
    }

    private void RecentList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (RecentList.SelectedItem is string character)
        {
            ViewModel.CopyRecentCharacterCommand.Execute(character);
        }
    }

    private void FavoriteList_KeyDown(object sender, KeyEventArgs e)
    {
        HandleQuickListKeyDown(FavoriteList, e);
    }

    private void FavoriteList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (FavoriteList.SelectedItem is string character)
        {
            ViewModel.CopyRecentCharacterCommand.Execute(character);
        }
    }

    private void HandleQuickListKeyDown(System.Windows.Controls.ListBox listBox, KeyEventArgs e)
    {
        var isRecent = listBox == RecentList;

        switch (e.Key)
        {
            case Key.Enter:
                // 선택된 문자 복사 + 붙여넣기
                if (listBox.SelectedItem is string character)
                {
                    ViewModel.CopyRecentCharacterCommand.Execute(character);
                }
                e.Handled = true;
                break;

            case Key.Left:
                if (listBox.SelectedIndex > 0)
                {
                    // 이전 아이템으로
                    listBox.SelectedIndex--;
                }
                else if (!isRecent && RecentList.Items.Count > 0 && RecentList.IsVisible)
                {
                    // 즐겨찾기 첫 번째에서 ← → 최근 마지막으로
                    RecentList.SelectedIndex = RecentList.Items.Count - 1;
                    RecentList.Focus();
                    HighlightSection("recent");
                }
                e.Handled = true;
                break;

            case Key.Right:
                if (listBox.SelectedIndex < listBox.Items.Count - 1)
                {
                    // 다음 아이템으로
                    listBox.SelectedIndex++;
                }
                else if (isRecent && FavoriteList.Items.Count > 0 && FavoriteList.IsVisible)
                {
                    // 최근 마지막에서 → → 즐겨찾기 첫 번째로
                    FavoriteList.SelectedIndex = 0;
                    FavoriteList.Focus();
                    HighlightSection("favorite");
                }
                e.Handled = true;
                break;

            case Key.Up:
                // 위로: 검색창으로
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                HighlightSection("search");
                e.Handled = true;
                break;

            case Key.Down:
                // 아래로: 카테고리로
                FocusOnCategory();
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    // Shift+Tab: 검색창으로
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
                    HighlightSection("search");
                }
                else
                {
                    // Tab: 카테고리로
                    FocusOnCategory();
                }
                e.Handled = true;
                break;
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        HideWindow();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }

    private void OpenSettings()
    {
        // 설정 창 열기 동안 Deactivated 무시
        _hideOnDeactivate = false;

        if (Application.Current is App app)
        {
            app.ShowSettingsWindow();
        }

        // 설정 창 닫힌 후 다시 활성화
        _hideOnDeactivate = true;
        _lastShowTime = DateTime.Now; // Deactivated 무시 타이머 재설정
    }

    private void CharacterGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 선택된 아이템이 보이도록 스크롤
        if (CharacterGrid.SelectedItem != null)
        {
            CharacterGrid.ScrollIntoView(CharacterGrid.SelectedItem);
        }

        // 그리드에서 선택 변경 시 섹션 하이라이트
        if (CharacterGrid.IsKeyboardFocusWithin || CharacterGrid.IsFocused)
        {
            HighlightSection("grid");
        }
    }

    private void UnicodeTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsGifMode = false;
    }

    private void GifTabButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsGifMode = true;
    }

    private void SwitchToPlugin(string pluginId)
    {
        if (pluginId == "unicode")
        {
            ViewModel.IsGifMode = false;
            UnicodeTabButton.IsChecked = true;
        }
        else if (pluginId == "gif" && ViewModel.IsGifPluginAvailable)
        {
            ViewModel.IsGifMode = true;
            GifTabButton.IsChecked = true;
        }
    }

    private void SwitchToNextPlugin()
    {
        if (!ViewModel.IsGifMode && ViewModel.IsGifPluginAvailable)
        {
            SwitchToPlugin("gif");
        }
        else
        {
            SwitchToPlugin("unicode");
        }
    }

    private void SwitchToPreviousPlugin()
    {
        // 현재는 2개뿐이라 Next와 동일
        SwitchToNextPlugin();
    }

    private void GifGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.SelectedGifResult != null)
        {
            // 더블클릭 = 복사 + 붙여넣기
            ViewModel.PasteGifAndCloseCommand.Execute(null);
        }
    }

    private void GifGrid_KeyDown(object sender, KeyEventArgs e)
    {
        if (ViewModel.SelectedGifResult != null)
        {
            if (e.Key == Key.Enter)
            {
                // Enter = 복사 + 붙여넣기
                ViewModel.PasteGifAndCloseCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+C = 복사만
                ViewModel.CopySelectedGifCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// 카테고리 스크롤뷰어 마우스 휠 처리 (수평 스크롤)
    /// </summary>
    private void CategoryScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            // 수평 스크롤로 변환 (휠 위로 = 왼쪽, 휠 아래로 = 오른쪽)
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }
    }

    /// <summary>
    /// 문자 그리드 스크롤뷰어 마우스 휠 처리
    /// </summary>
    private void CharacterScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
