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

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is PanelViewModel vm)
        {
            var hotkey = vm.Panel.Hotkey ?? "Ctrl+Shift+Space";
            MessageBox.Show(
                $"非表示にします。\n\n再度表示したいときは {hotkey} を押すか、\nシステムトレイのアイコンをクリックしてください。",
                "非表示",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            vm.Hide();
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "表示/非表示を推奨しますが、アプリを終了してよろしいですか？",
            "終了確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }
}
