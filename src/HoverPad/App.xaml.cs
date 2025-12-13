using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using HoverPad.Services;
using HoverPad.ViewModels;
using HoverPad.Views;

namespace HoverPad;

public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private IServiceProvider? _serviceProvider;
    private readonly List<PanelWindow> _panelWindows = new();
    private Window? _hotkeyWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

        CreateHotkeyWindow();

        var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
        var handle = new WindowInteropHelper(_hotkeyWindow!).Handle;
        hotkeyService.Initialize(handle);

        await mainViewModel.InitializeAsync();

        var configService = _serviceProvider.GetRequiredService<IConfigService>();

        foreach (var panelVm in mainViewModel.Panels)
        {
            var window = new PanelWindow(panelVm);
            _panelWindows.Add(window);

            // 設定ボタンのイベント
            var capturedVm = panelVm;
            var capturedWindow = window;
            window.SettingsRequested += (s, e) =>
            {
                // 設定画面を開く前にパネルを非表示
                capturedWindow.Hide();

                var settingsWindow = new SettingsWindow(capturedVm, configService);
                settingsWindow.ShowDialog();

                // 設定画面を閉じたらパネルを再表示
                capturedVm.RefreshButtons();
                capturedWindow.Show();
            };

            if (panelVm.IsVisible)
            {
                window.Show();
            }
        }

        SetupTrayIcon(mainViewModel);

        // 起動時に最初のパネルを表示
        if (_panelWindows.Count > 0)
        {
            var firstPanel = _panelWindows[0];
            var vm = mainViewModel.Panels[0];
            vm.Show();
            firstPanel.Show();
            firstPanel.Activate();
        }
    }

    private void CreateHotkeyWindow()
    {
        _hotkeyWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Visibility = Visibility.Hidden
        };
        _hotkeyWindow.Show();
        _hotkeyWindow.Hide();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IActionExecutor, ActionExecutor>();
        services.AddSingleton<MainViewModel>();
    }

    private void SetupTrayIcon(MainViewModel mainViewModel)
    {
        _trayIcon = new TaskbarIcon
        {
            Icon = SystemIcons.Application,
            ToolTipText = "HoverPad - Ctrl+Shift+Space でパネル表示",
            ContextMenu = CreateTrayContextMenu(mainViewModel)
        };

        _trayIcon.TrayMouseDoubleClick += (s, e) =>
        {
            mainViewModel.ShowAllPanels();
        };
    }

    private System.Windows.Controls.ContextMenu CreateTrayContextMenu(MainViewModel mainViewModel)
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var showAllItem = new System.Windows.Controls.MenuItem { Header = "すべてのパネルを表示" };
        showAllItem.Click += (s, e) => mainViewModel.ShowAllPanels();
        menu.Items.Add(showAllItem);

        var hideAllItem = new System.Windows.Controls.MenuItem { Header = "すべてのパネルを非表示" };
        hideAllItem.Click += (s, e) => mainViewModel.HideAllPanels();
        menu.Items.Add(hideAllItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var settingsItem = new System.Windows.Controls.MenuItem { Header = "設定..." };
        settingsItem.Click += (s, e) => mainViewModel.OpenSettingsCommand.Execute(null);
        menu.Items.Add(settingsItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "終了" };
        exitItem.Click += (s, e) => mainViewModel.ExitCommand.Execute(null);
        menu.Items.Add(exitItem);

        return menu;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
        base.OnExit(e);
    }
}
