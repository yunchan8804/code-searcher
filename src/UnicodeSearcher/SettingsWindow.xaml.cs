using System.Windows;
using System.Windows.Input;
using UnicodeSearcher.ViewModels;

namespace UnicodeSearcher;

/// <summary>
/// SettingsWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class SettingsWindow : Window
{
    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    private bool _isCapturingHotkey = false;

    public SettingsWindow()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SaveCompleted += OnSaveCompleted;
                viewModel.Cancelled += OnCancelled;
            }
        };

        Unloaded += (_, _) =>
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SaveCompleted -= OnSaveCompleted;
                viewModel.Cancelled -= OnCancelled;
            }
        };
    }

    private void OnSaveCompleted(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isCapturingHotkey) return;

        e.Handled = true;

        // 단독 모디파이어 키는 무시
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LWin || e.Key == Key.RWin ||
            e.Key == Key.System)
        {
            return;
        }

        // ESC는 취소
        if (e.Key == Key.Escape)
        {
            _isCapturingHotkey = false;
            HotkeyTextBox.Background = System.Windows.Media.Brushes.WhiteSmoke;
            return;
        }

        // 모디파이어 확인
        var modifiers = Keyboard.Modifiers;

        // 최소 하나의 모디파이어 필요
        if (modifiers == ModifierKeys.None)
        {
            MessageBox.Show("Ctrl, Alt, Shift 중 하나 이상과 함께 키를 눌러주세요.",
                "핫키 설정", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 핫키 설정
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        ViewModel.SetHotkey(modifiers, key);

        _isCapturingHotkey = false;
        HotkeyTextBox.Background = System.Windows.Media.Brushes.WhiteSmoke;
    }

    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = true;
        HotkeyTextBox.Background = System.Windows.Media.Brushes.LightYellow;
        HotkeyTextBox.Text = "키를 입력하세요...";
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _isCapturingHotkey = false;
        HotkeyTextBox.Background = System.Windows.Media.Brushes.WhiteSmoke;
        // 원래 핫키 표시 복원
        HotkeyTextBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)?.UpdateTarget();
    }
}
