using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        // 검색창에 포커스
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();

        // ViewModel 이벤트 구독
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.CloseWindowRequested += OnCloseWindowRequested;
            viewModel.PasteRequested += OnPasteRequested;
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

    private void OnPasteRequested(object? sender, EventArgs e)
    {
        Helpers.DebugLogger.Log("OnPasteRequested START");

        Helpers.DebugLogger.Log("Before HideWindow");
        HideWindow();
        Helpers.DebugLogger.Log("After HideWindow");

        // 완전히 백그라운드 스레드에서 실행
        Task.Run(async () =>
        {
            Helpers.DebugLogger.Log("Task.Run START");
            await Task.Delay(150);
            Helpers.DebugLogger.Log("Before PasteToActiveWindow");
            WindowHelper.PasteToActiveWindow();
            Helpers.DebugLogger.Log("After PasteToActiveWindow");
        });

        Helpers.DebugLogger.Log("OnPasteRequested END");
    }

    /// <summary>
    /// 창 표시
    /// </summary>
    public void ShowWindow(bool positionNearCursor = true)
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
        Topmost = true;  // 일시적으로 최상단
        Activate();

        // 잠시 후 Topmost 해제 및 포커스 설정
        Dispatcher.BeginInvoke(() =>
        {
            Topmost = true;  // 계속 최상단 유지 (원래 설정)
            Focus();
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    /// <summary>
    /// 창 숨기기
    /// </summary>
    public void HideWindow()
    {
        Helpers.DebugLogger.Log("HideWindow START");
        _hideOnDeactivate = false;
        Helpers.DebugLogger.Log("Before Hide()");
        Hide();
        Helpers.DebugLogger.Log("After Hide()");

        // 창 숨긴 후 백그라운드에서 정리
        Dispatcher.BeginInvoke(() => ViewModel.OnWindowClosing(), DispatcherPriority.Background);
        Helpers.DebugLogger.Log("HideWindow END");
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

        // Ctrl + 방향키: 카테고리 이동, Ctrl+D: 즐겨찾기 토글
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
        }

        // 숫자키: 최근 사용 문자 (검색어 비었을 때)
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

        // 카테고리 탭에 포커스가 있을 때
        if (CategoryTabs.IsFocused)
        {
            HandleCategoryKeyDown(e);
            return;
        }

        // 그리드 탐색
        HandleGridKeyDown(e);
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
                // 카테고리 탭으로 포커스 이동
                FocusOnCategory();
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    FocusOnCategory();
                    e.Handled = true;
                }
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
                // 검색창으로 포커스 이동
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
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
                    SearchTextBox.Focus();
                }
                else
                {
                    FocusOnGrid();
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
                    SearchTextBox.Focus();
                    e.Handled = true;
                }
                break;

            default:
                // 알파벳/한글 입력 시 검색창으로 포커스
                if (IsTypingKey(e.Key))
                {
                    SearchTextBox.Focus();
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
        // 그리드 자체에 포커스를 주어 키보드 네비게이션 활성화
        CharacterGrid.Focus();
    }

    private void FocusOnCategory()
    {
        // 카테고리 탭에 포커스
        CategoryTabs.Focus();
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

    private void CharacterGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 선택된 아이템이 보이도록 스크롤
        if (CharacterGrid.SelectedItem != null)
        {
            CharacterGrid.ScrollIntoView(CharacterGrid.SelectedItem);
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
