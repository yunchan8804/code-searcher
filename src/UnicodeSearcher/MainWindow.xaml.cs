using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

    /// <summary>
    /// 창 표시
    /// </summary>
    public void ShowWindow(bool positionNearCursor = true)
    {
        _hideOnDeactivate = true;

        if (positionNearCursor)
        {
            WindowHelper.PositionNearCursor(this);
        }

        WindowHelper.ActivateWindow(this);

        // 검색창 포커스 및 전체 선택
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();
    }

    /// <summary>
    /// 창 숨기기
    /// </summary>
    public void HideWindow()
    {
        _hideOnDeactivate = false;
        ViewModel.OnWindowClosing();
        Hide();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
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

        // Ctrl + 방향키: 카테고리 이동
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

        // 그리드 탐색
        HandleGridKeyDown(e);
    }

    private void HandleSearchBoxKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                // 첫 번째 결과 복사 + 닫기
                if (ViewModel.FilteredCharacters.Count > 0)
                {
                    ViewModel.CopyFirstResultCommand.Execute(null);
                }
                e.Handled = true;
                break;

            case Key.Down:
                // 그리드로 포커스 이동
                if (ViewModel.FilteredCharacters.Count > 0)
                {
                    ViewModel.SelectedIndex = 0;
                    FocusOnGrid();
                }
                e.Handled = true;
                break;

            case Key.Tab:
                if (Keyboard.Modifiers != ModifierKeys.Shift)
                {
                    FocusOnGrid();
                    e.Handled = true;
                }
                break;
        }
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
                    // 첫 행에서 위로 가면 검색창으로
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
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
                // 복사 + 창 닫기
                ViewModel.CopyAndCloseCommand.Execute(null);
                e.Handled = true;
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

    private void CharacterItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is UnicodeCharacter character)
        {
            ViewModel.SelectedCharacter = character;

            // 더블클릭 처리
            if (e.ClickCount == 2)
            {
                ViewModel.CopyCharacterCommand.Execute(character);
            }
        }
    }

    private void CharacterItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 싱글클릭: 선택만 (복사는 더블클릭 또는 버튼/Enter로)
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
