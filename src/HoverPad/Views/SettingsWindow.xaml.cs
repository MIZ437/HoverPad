using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using HoverPad.Models;
using HoverPad.Services;
using HoverPad.ViewModels;

namespace HoverPad.Views;

public partial class SettingsWindow : Window
{
    private readonly PanelViewModel _panelViewModel;
    private readonly IConfigService _configService;
    private PanelButton? _selectedButton;
    private readonly string _presetsPath;

    public SettingsWindow(PanelViewModel panelViewModel, IConfigService configService)
    {
        InitializeComponent();
        _panelViewModel = panelViewModel;
        _configService = configService;

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _presetsPath = Path.Combine(appDataPath, "HoverPad", "presets");
        Directory.CreateDirectory(_presetsPath);

        LoadSettings();
        LoadPresetList();
    }

    private void LoadSettings()
    {
        // グリッドサイズの設定
        RowsComboBox.SelectedIndex = Math.Min(_panelViewModel.Panel.Rows - 1, 9);
        ColsComboBox.SelectedIndex = Math.Min(_panelViewModel.Panel.Cols - 1, 9);

        // ボタンリストの更新
        RefreshButtonList();
        UpdateGridInfo();
    }

    private void RefreshButtonList()
    {
        ButtonListBox.ItemsSource = null;
        ButtonListBox.ItemsSource = _panelViewModel.Panel.Buttons;
    }

    private void UpdateGridInfo()
    {
        if (!IsLoaded) return;
        var rows = RowsComboBox.SelectedIndex + 1;
        var cols = ColsComboBox.SelectedIndex + 1;
        var total = rows * cols;
        var current = _panelViewModel.Panel.Buttons.Count;
        GridInfoText.Text = $"(最大 {total} ボタン, 現在 {current} 個)";
    }

    private void GridSize_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateGridInfo();
    }

    #region プリセット管理

    private void LoadPresetList()
    {
        PresetComboBox.Items.Clear();
        if (Directory.Exists(_presetsPath))
        {
            foreach (var file in Directory.GetFiles(_presetsPath, "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                PresetComboBox.Items.Add(name);
            }
        }
    }

    private void LoadPreset_Click(object sender, RoutedEventArgs e)
    {
        if (PresetComboBox.SelectedItem == null)
        {
            MessageBox.Show("読み込むプリセットを選択してください。", "プリセット", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = PresetComboBox.SelectedItem.ToString()!;
        var path = Path.Combine(_presetsPath, $"{name}.json");

        try
        {
            var json = File.ReadAllText(path);
            var preset = JsonSerializer.Deserialize<PresetData>(json);
            if (preset != null)
            {
                _panelViewModel.Panel.Rows = preset.Rows;
                _panelViewModel.Panel.Cols = preset.Cols;
                _panelViewModel.Panel.Buttons = preset.Buttons;

                RowsComboBox.SelectedIndex = Math.Min(preset.Rows - 1, 9);
                ColsComboBox.SelectedIndex = Math.Min(preset.Cols - 1, 9);
                RefreshButtonList();
                UpdateGridInfo();

                MessageBox.Show($"プリセット「{name}」を読み込みました。", "プリセット", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"プリセットの読み込みに失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog("プリセット名を入力", "プリセット保存");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
        {
            var name = dialog.InputText.Trim();
            // ファイル名に使えない文字を除去
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), "");
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("有効なプリセット名を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var path = Path.Combine(_presetsPath, $"{name}.json");

            // 現在の設定を一時的に更新
            _panelViewModel.Panel.Rows = RowsComboBox.SelectedIndex + 1;
            _panelViewModel.Panel.Cols = ColsComboBox.SelectedIndex + 1;

            var preset = new PresetData
            {
                Rows = _panelViewModel.Panel.Rows,
                Cols = _panelViewModel.Panel.Cols,
                Buttons = _panelViewModel.Panel.Buttons
            };

            var json = JsonSerializer.Serialize(preset, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);

            LoadPresetList();
            PresetComboBox.SelectedItem = name;

            MessageBox.Show($"プリセット「{name}」を保存しました。", "プリセット", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (PresetComboBox.SelectedItem == null)
        {
            MessageBox.Show("削除するプリセットを選択してください。", "プリセット", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = PresetComboBox.SelectedItem.ToString()!;
        var result = MessageBox.Show($"プリセット「{name}」を削除しますか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var path = Path.Combine(_presetsPath, $"{name}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                LoadPresetList();
                MessageBox.Show($"プリセット「{name}」を削除しました。", "プリセット", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    #endregion

    #region ボタン管理

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var rows = RowsComboBox.SelectedIndex + 1;
        var cols = ColsComboBox.SelectedIndex + 1;
        var maxButtons = rows * cols;

        if (_panelViewModel.Panel.Buttons.Count >= maxButtons)
        {
            MessageBox.Show($"グリッドがいっぱいです。最大 {maxButtons} 個のボタンを配置できます。\nグリッドサイズを大きくしてください。",
                "ボタン追加", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 空いている位置を探す
        var usedPositions = _panelViewModel.Panel.Buttons
            .Select(b => (b.Position.Row, b.Position.Col))
            .ToHashSet();

        int newRow = 0, newCol = 0;
        bool found = false;
        for (int r = 0; r < rows && !found; r++)
        {
            for (int c = 0; c < cols && !found; c++)
            {
                if (!usedPositions.Contains((r, c)))
                {
                    newRow = r;
                    newCol = c;
                    found = true;
                }
            }
        }

        var newButton = new PanelButton
        {
            Id = Guid.NewGuid(),
            Position = new GridPosition { Row = newRow, Col = newCol },
            Label = "新規",
            Icon = "\u2B50",
            Actions = new List<ButtonAction>()
        };

        _panelViewModel.Panel.Buttons.Add(newButton);
        RefreshButtonList();
        UpdateGridInfo();
        ButtonListBox.SelectedItem = newButton;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedButton == null)
        {
            MessageBox.Show("削除するボタンを選択してください。", "削除", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show($"ボタン「{_selectedButton.Label}」を削除しますか？",
            "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _panelViewModel.Panel.Buttons.Remove(_selectedButton);
            _selectedButton = null;
            RefreshButtonList();
            UpdateGridInfo();
            ClearButtonEditor();
        }
    }

    private void ButtonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedButton = ButtonListBox.SelectedItem as PanelButton;
        if (_selectedButton != null)
        {
            LoadButtonToEditor(_selectedButton);
        }
    }

    private void LoadButtonToEditor(PanelButton button)
    {
        LabelTextBox.Text = button.Label;
        IconTextBox.Text = button.Icon;

        // アクションタイプの設定
        if (button.Actions.Count > 0)
        {
            var action = button.Actions[0];
            ActionTypeComboBox.SelectedIndex = action.Type switch
            {
                ActionType.Hotkey => 0,
                ActionType.Open => 1,
                ActionType.Command => 2,
                ActionType.Text => 3,
                _ => 0
            };

            // アクション内容の表示
            ActionTextBox.Text = GetActionText(action);
        }
        else
        {
            ActionTypeComboBox.SelectedIndex = 0;
            ActionTextBox.Text = "";
        }

        // ヘルプテキストを更新
        UpdateActionHelpText();
    }

    private void UpdateActionHelpText()
    {
        if (ActionHelpText == null) return;

        var tag = (ActionTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        ActionHelpText.Text = tag switch
        {
            "Hotkey" => "例: Ctrl+C, Ctrl+Shift+V, Alt+Tab\nキーボードショートカットを送信します",
            "Open" => "例: notepad.exe, C:\\Program Files\\app.exe, https://google.com\nアプリ、ファイル、URLを開きます",
            "Command" => "例: dir, ipconfig, echo Hello\nコマンドプロンプトでコマンドを実行します",
            "Text" => "入力したいテキストを記入\nクリップボード経由でテキストを貼り付けます",
            _ => ""
        };
    }

    private static string GetActionText(ButtonAction action)
    {
        if (action.Payload == null) return "";

        try
        {
            return action.Type switch
            {
                ActionType.Hotkey => JsonSerializer.Deserialize<HotkeyPayload>(action.Payload.Value.GetRawText())?.Keys ?? "",
                ActionType.Open => JsonSerializer.Deserialize<OpenPayload>(action.Payload.Value.GetRawText())?.Path ?? "",
                ActionType.Command => JsonSerializer.Deserialize<CommandPayload>(action.Payload.Value.GetRawText())?.Command ?? "",
                ActionType.Text => JsonSerializer.Deserialize<TextPayload>(action.Payload.Value.GetRawText())?.Text ?? "",
                _ => action.Payload.Value.GetRawText()
            };
        }
        catch
        {
            return "";
        }
    }

    private void ClearButtonEditor()
    {
        LabelTextBox.Text = "";
        IconTextBox.Text = "";
        ActionTypeComboBox.SelectedIndex = 0;
        ActionTextBox.Text = "";
    }

    private void ActionType_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateActionHelpText();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedButton == null)
        {
            MessageBox.Show("編集するボタンを選択してください。", "保存", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _selectedButton.Label = LabelTextBox.Text;
        _selectedButton.Icon = IconTextBox.Text;

        // アクションの保存
        var actionType = (ActionTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() switch
        {
            "Hotkey" => ActionType.Hotkey,
            "Open" => ActionType.Open,
            "Command" => ActionType.Command,
            "Text" => ActionType.Text,
            _ => ActionType.Hotkey
        };

        var payload = CreatePayload(actionType, ActionTextBox.Text);

        _selectedButton.Actions.Clear();
        _selectedButton.Actions.Add(new ButtonAction
        {
            Type = actionType,
            Payload = payload,
            DelayMs = 0
        });

        RefreshButtonList();
        MessageBox.Show("ボタンを保存しました。", "保存", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static JsonElement? CreatePayload(ActionType type, string content)
    {
        object payloadObj = type switch
        {
            ActionType.Hotkey => new HotkeyPayload { Keys = content },
            ActionType.Open => new OpenPayload { Path = content },
            ActionType.Command => new CommandPayload { Command = content },
            ActionType.Text => new TextPayload { Text = content },
            _ => new HotkeyPayload { Keys = content }
        };

        return JsonSerializer.SerializeToElement(payloadObj);
    }

    #endregion

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void SaveAndClose_Click(object sender, RoutedEventArgs e)
    {
        // グリッドサイズの更新
        _panelViewModel.Panel.Rows = RowsComboBox.SelectedIndex + 1;
        _panelViewModel.Panel.Cols = ColsComboBox.SelectedIndex + 1;

        // ViewModelのButtonsを更新
        _panelViewModel.RefreshButtons();

        // 設定をファイルに保存
        await _configService.SaveAsync();

        MessageBox.Show("設定を保存しました。", "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }
}

// プリセットデータ
public class PresetData
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public List<PanelButton> Buttons { get; set; } = new();
}

// 入力ダイアログ
public class InputDialog : Window
{
    private readonly TextBox _textBox;
    public string InputText => _textBox.Text;

    public InputDialog(string prompt, string title)
    {
        Title = title;
        Width = 400;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x1E));
        Topmost = true;

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new TextBlock
        {
            Text = prompt,
            Foreground = System.Windows.Media.Brushes.White,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        _textBox = new TextBox
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3D, 0x3D, 0x3D)),
            Foreground = System.Windows.Media.Brushes.White,
            Padding = new Thickness(8, 5, 8, 5),
            FontSize = 14
        };
        Grid.SetRow(_textBox, 1);
        grid.Children.Add(_textBox);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 15, 0, 0)
        };
        Grid.SetRow(buttonPanel, 2);

        var cancelButton = new Button
        {
            Content = "キャンセル",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(0, 0, 10, 0),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0)
        };
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
        buttonPanel.Children.Add(cancelButton);

        var okButton = new Button
        {
            Content = "OK",
            Padding = new Thickness(15, 8, 15, 8),
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x34, 0x98, 0xDB)),
            Foreground = System.Windows.Media.Brushes.White,
            BorderThickness = new Thickness(0)
        };
        okButton.Click += (s, e) => { DialogResult = true; Close(); };
        buttonPanel.Children.Add(okButton);

        grid.Children.Add(buttonPanel);
        Content = grid;

        _textBox.Focus();
    }
}
