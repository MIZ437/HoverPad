using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HoverPad.Models;
using HoverPad.Services;

namespace HoverPad.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly IHotkeyService _hotkeyService;
    private readonly IActionExecutor _actionExecutor;

    [ObservableProperty]
    private ObservableCollection<PanelViewModel> _panels = new();

    public MainViewModel(IConfigService configService, IHotkeyService hotkeyService, IActionExecutor actionExecutor)
    {
        _configService = configService;
        _hotkeyService = hotkeyService;
        _actionExecutor = actionExecutor;
    }

    public async Task InitializeAsync()
    {
        await _configService.LoadAsync();
        LoadPanels();
    }

    private void LoadPanels()
    {
        Panels.Clear();
        var hotkeyId = 1;
        foreach (var panel in _configService.Config.Panels)
        {
            var viewModel = new PanelViewModel(panel, _actionExecutor, _configService);
            Panels.Add(viewModel);

            if (!string.IsNullOrEmpty(panel.Hotkey))
            {
                _hotkeyService.Register(hotkeyId, panel.Hotkey);
                var capturedViewModel = viewModel;
                var capturedId = hotkeyId;
                _hotkeyService.HotkeyPressed += (s, e) =>
                {
                    if (e.Id == capturedId)
                    {
                        capturedViewModel.ToggleCommand.Execute(null);
                    }
                };
                hotkeyId++;
            }

            if (panel.Mode == PanelMode.Docked)
            {
                viewModel.Show();
            }
        }
    }

    [RelayCommand]
    public void ShowAllPanels()
    {
        foreach (var panel in Panels)
        {
            panel.Show();
        }
    }

    [RelayCommand]
    public void HideAllPanels()
    {
        foreach (var panel in Panels)
        {
            panel.Hide();
        }
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        await _configService.SaveAsync();
    }

    [RelayCommand]
    public void OpenSettings()
    {
        // TODO: 設定画面を開く
    }

    [RelayCommand]
    public void Exit()
    {
        _hotkeyService.UnregisterAll();
        System.Windows.Application.Current.Shutdown();
    }
}
