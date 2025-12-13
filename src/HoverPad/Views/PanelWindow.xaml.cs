using System.Windows;
using System.Windows.Input;
using HoverPad.ViewModels;

namespace HoverPad.Views;

public partial class PanelWindow : Window
{
    public event EventHandler? SettingsRequested;

    public PanelWindow()
    {
        InitializeComponent();
    }

    public PanelWindow(PanelViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PanelViewModel.IsVisible))
        {
            var vm = (PanelViewModel)sender!;
            if (vm.IsVisible)
            {
                Show();
                Activate();
            }
            else
            {
                Hide();
            }
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
            if (DataContext is PanelViewModel vm)
            {
                vm.UpdatePosition(Left, Top);
            }
        }
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // フォーカス喪失時の自動非表示は無効化
        // Escapeキーまたはホットキーで閉じる
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && DataContext is PanelViewModel vm)
        {
            vm.Hide();
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }
}
