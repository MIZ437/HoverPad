using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HoverPad.Infrastructure;
using HoverPad.Models;
using HoverPad.Services;

namespace HoverPad.ViewModels;

public partial class PanelViewModel : ObservableObject
{
    private readonly IActionExecutor _actionExecutor;
    private readonly IConfigService _configService;

    [ObservableProperty]
    private Panel _panel;

    [ObservableProperty]
    private ObservableCollection<ButtonViewModel> _buttons = new();

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private double _left;

    [ObservableProperty]
    private double _top;

    public PanelViewModel(Panel panel, IActionExecutor actionExecutor, IConfigService configService)
    {
        _panel = panel;
        _actionExecutor = actionExecutor;
        _configService = configService;
        LoadButtons();
    }

    private void LoadButtons()
    {
        Buttons.Clear();
        foreach (var button in Panel.Buttons)
        {
            Buttons.Add(new ButtonViewModel(button, _actionExecutor, this));
        }
    }

    public void RefreshButtons()
    {
        LoadButtons();
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    public void Show()
    {
        // 画面中央に表示
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = (screenWidth - 300) / 2;
        Top = (screenHeight - 300) / 2;
        IsVisible = true;
    }

    [RelayCommand]
    public void Hide()
    {
        IsVisible = false;
    }

    [RelayCommand]
    public void Toggle()
    {
        if (IsVisible)
            Hide();
        else
            Show();
    }

    public void UpdatePosition(double left, double top)
    {
        Left = left;
        Top = top;
        Panel.Position.X = left;
        Panel.Position.Y = top;
    }

    public async Task OnActionExecutedAsync()
    {
        if (_configService.Config.Settings.HideOnActionExecute && Panel.Mode == PanelMode.Dynamic)
        {
            await Task.Delay(100);
            Hide();
        }
    }
}

public partial class ButtonViewModel : ObservableObject
{
    private readonly IActionExecutor _actionExecutor;
    private readonly PanelViewModel _panelViewModel;

    [ObservableProperty]
    private PanelButton _button;

    public int Row => Button.Position.Row;
    public int Col => Button.Position.Col;

    public ButtonViewModel(PanelButton button, IActionExecutor actionExecutor, PanelViewModel panelViewModel)
    {
        _button = button;
        _actionExecutor = actionExecutor;
        _panelViewModel = panelViewModel;
    }

    [RelayCommand]
    public async Task ExecuteAsync()
    {
        if (Button.Actions == null || Button.Actions.Count == 0)
        {
            return;
        }

        if (Button.IsToggle)
        {
            Button.ToggleState = !Button.ToggleState;
            OnPropertyChanged(nameof(Button));
        }

        await _actionExecutor.ExecuteAsync(Button.Actions);
        await _panelViewModel.OnActionExecutedAsync();
    }
}
