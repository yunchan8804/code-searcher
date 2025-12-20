using System.Windows;
using UnicodeSearcher.Services;
using UnicodeSearcher.ViewModels;

namespace UnicodeSearcher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 서비스 생성
        var characterDataService = new CharacterDataService();
        var searchService = new SearchService();
        var clipboardService = new ClipboardService();

        // ViewModel 생성
        var viewModel = new MainViewModel(
            characterDataService,
            searchService,
            clipboardService);

        // MainWindow 생성 및 표시
        var mainWindow = new MainWindow
        {
            DataContext = viewModel
        };

        mainWindow.Show();
    }
}
