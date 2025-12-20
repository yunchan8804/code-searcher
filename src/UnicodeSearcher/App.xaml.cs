using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using UnicodeSearcher.Helpers;
using UnicodeSearcher.Services;
using UnicodeSearcher.ViewModels;

namespace UnicodeSearcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private MainViewModel? _viewModel;
    private IHotkeyService? _hotkeyService;
    private ISettingsService? _settingsService;
    private IRecentCharactersService? _recentCharactersService;
    private IFavoriteService? _favoriteService;
    private IThemeService? _themeService;
    private IStartupService? _startupService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 전역 예외 핸들러
        DispatcherUnhandledException += (s, args) =>
        {
            Console.WriteLine($"[UI 오류] {args.Exception.Message}");
            Console.WriteLine(args.Exception.StackTrace);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            Console.WriteLine($"[전역 오류] {ex?.Message}");
            Console.WriteLine(ex?.StackTrace);
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            Console.WriteLine($"[Task 오류] {args.Exception.Message}");
            Console.WriteLine(args.Exception.StackTrace);
            args.SetObserved();
        };

        // 서비스 생성
        var characterDataService = new CharacterDataService();
        var searchService = new SearchService();
        var clipboardService = new ClipboardService();
        _recentCharactersService = new RecentCharactersService();
        _settingsService = new SettingsService();
        _favoriteService = new FavoriteService();
        _themeService = new ThemeService();
        _hotkeyService = new HotkeyService();
        _startupService = new StartupService();

        // ViewModel 생성
        _viewModel = new MainViewModel(
            characterDataService,
            searchService,
            clipboardService,
            _recentCharactersService,
            _settingsService,
            _favoriteService);

        // MainWindow 생성
        _mainWindow = new MainWindow
        {
            DataContext = _viewModel
        };

        // 시스템 트레이 아이콘 설정
        SetupTrayIcon();

        // 글로벌 핫키 설정
        SetupHotkey();

        // 테마 설정 적용
        SetupTheme();

        // --minimized 인자 확인
        var startMinimized = e.Args.Contains("--minimized");

        // 창 표시 (--minimized 인자가 없으면 표시)
        if (!startMinimized)
        {
            _mainWindow.Show();
        }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "유니코드 검색기 (Ctrl+Alt+Space)",
            Visibility = Visibility.Visible
        };

        // 텍스트 기반 아이콘 생성 (실제 아이콘 대신)
        var iconText = new System.Windows.Controls.TextBlock
        {
            Text = "★",
            FontSize = 14,
            Foreground = System.Windows.Media.Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // 컨텍스트 메뉴 생성
        var contextMenu = new System.Windows.Controls.ContextMenu();

        var openMenuItem = new System.Windows.Controls.MenuItem { Header = "열기 (Ctrl+Alt+Space)" };
        openMenuItem.Click += (_, _) => ShowMainWindow();
        contextMenu.Items.Add(openMenuItem);

        var settingsMenuItem = new System.Windows.Controls.MenuItem { Header = "설정..." };
        settingsMenuItem.Click += (_, _) => ShowSettingsWindow();
        contextMenu.Items.Add(settingsMenuItem);

        contextMenu.Items.Add(new System.Windows.Controls.Separator());

        var exitMenuItem = new System.Windows.Controls.MenuItem { Header = "종료" };
        exitMenuItem.Click += (_, _) => ExitApplication();
        contextMenu.Items.Add(exitMenuItem);

        _trayIcon.ContextMenu = contextMenu;

        // 더블클릭으로 창 표시
        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();
    }

    private void SetupHotkey()
    {
        if (_hotkeyService == null || _settingsService == null) return;

        // 핫키 등록 (Ctrl + Alt + Space)
        var modifiers = _settingsService.Settings.Hotkey.ModifierKeys;
        var key = _settingsService.Settings.Hotkey.KeyValue;

        _hotkeyService.RegisterHotkey(modifiers, key);

        // 핫키 이벤트 핸들러
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        // 핫키 시작
        _hotkeyService.Start();
    }

    private void SetupTheme()
    {
        if (_themeService == null || _settingsService == null) return;

        // 설정에서 테마 모드 가져와서 적용
        var themeMode = _settingsService.Settings.Appearance.Theme switch
        {
            "Dark" => ThemeMode.Dark,
            "Light" => ThemeMode.Light,
            _ => ThemeMode.System
        };

        _themeService.SetTheme(themeMode);
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (_mainWindow == null) return;

        // 현재 활성 창 저장
        WindowHelper.SaveActiveWindow();

        // 창이 표시되어 있으면 숨기고, 아니면 표시
        if (_mainWindow.IsVisible)
        {
            _mainWindow.HideWindow();
        }
        else
        {
            _mainWindow.ShowWindow(positionNearCursor: true);
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null) return;

        WindowHelper.SaveActiveWindow();
        _mainWindow.ShowWindow(positionNearCursor: false);
    }

    private void ShowSettingsWindow()
    {
        if (_settingsService == null || _themeService == null || _startupService == null) return;

        var settingsViewModel = new ViewModels.SettingsViewModel(_settingsService, _themeService, _startupService);
        var settingsWindow = new SettingsWindow
        {
            DataContext = settingsViewModel,
            Owner = _mainWindow
        };

        var result = settingsWindow.ShowDialog();

        if (result == true)
        {
            // 핫키 재등록
            ReregisterHotkey();
        }
    }

    private void ReregisterHotkey()
    {
        if (_hotkeyService == null || _settingsService == null) return;

        // 기존 핫키 해제 및 새 핫키 등록
        _hotkeyService.Stop();

        var modifiers = _settingsService.Settings.Hotkey.ModifierKeys;
        var key = _settingsService.Settings.Hotkey.KeyValue;

        _hotkeyService.RegisterHotkey(modifiers, key);
        _hotkeyService.Start();
    }

    private async void ExitApplication()
    {
        // 상태 저장
        if (_viewModel != null)
        {
            await _viewModel.SaveStateAsync();
        }

        // 핫키 해제
        _hotkeyService?.Dispose();

        // 트레이 아이콘 제거
        _trayIcon?.Dispose();

        // 앱 종료
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
