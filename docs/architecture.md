# HoverPad アーキテクチャ設計書

## 1. 技術スタック

| カテゴリ | 技術 | バージョン |
|----------|------|-----------|
| フレームワーク | WPF | .NET 8 |
| 言語 | C# | 12 |
| 設定管理 | JSON | System.Text.Json |
| ホットキー | Windows API | user32.dll |
| 依存性注入 | Microsoft.Extensions.DependencyInjection | 8.x |
| MVVM | CommunityToolkit.Mvvm | 8.x |

## 2. プロジェクト構造

```
HoverPad/
├── src/
│   ├── HoverPad/                    # メインアプリケーション
│   │   ├── App.xaml                 # アプリケーション定義
│   │   ├── App.xaml.cs
│   │   ├── Views/                   # XAML Views
│   │   │   ├── PanelWindow.xaml     # オーバーレイパネル
│   │   │   ├── SettingsWindow.xaml  # 設定画面
│   │   │   ├── ButtonEditWindow.xaml # ボタン編集
│   │   │   └── ActionEditWindow.xaml # アクション編集
│   │   ├── ViewModels/              # ViewModels (MVVM)
│   │   │   ├── PanelViewModel.cs
│   │   │   ├── SettingsViewModel.cs
│   │   │   ├── ButtonEditViewModel.cs
│   │   │   └── ActionEditViewModel.cs
│   │   ├── Models/                  # データモデル
│   │   │   ├── Panel.cs
│   │   │   ├── Button.cs
│   │   │   ├── Action.cs
│   │   │   ├── Profile.cs
│   │   │   └── AppSettings.cs
│   │   ├── Services/                # ビジネスロジック
│   │   │   ├── IConfigService.cs
│   │   │   ├── ConfigService.cs
│   │   │   ├── IHotkeyService.cs
│   │   │   ├── HotkeyService.cs
│   │   │   ├── IActionExecutor.cs
│   │   │   ├── ActionExecutor.cs
│   │   │   ├── IProfileService.cs
│   │   │   └── ProfileService.cs
│   │   ├── Infrastructure/          # インフラストラクチャ
│   │   │   ├── NativeMethods.cs     # Windows API
│   │   │   ├── GlobalHotkey.cs
│   │   │   └── WindowHelper.cs
│   │   ├── Converters/              # 値コンバーター
│   │   │   └── BoolToVisibilityConverter.cs
│   │   ├── Resources/               # リソース
│   │   │   ├── Styles.xaml
│   │   │   └── Icons/
│   │   └── HoverPad.csproj
│   │
│   └── HoverPad.Core/               # コアライブラリ（将来の拡張用）
│       ├── Abstractions/
│       └── HoverPad.Core.csproj
│
├── tests/
│   └── HoverPad.Tests/              # 単体テスト
│       └── HoverPad.Tests.csproj
│
├── docs/                            # ドキュメント
│   ├── architecture.md
│   └── user-guide.md
│
├── HoverPad.sln                     # ソリューションファイル
└── README.md
```

## 3. アーキテクチャ概要

### 3.1 レイヤー構成

```
┌─────────────────────────────────────────────────────┐
│                    Views (XAML)                     │
│  PanelWindow | SettingsWindow | ButtonEditWindow   │
└─────────────────────────────────────────────────────┘
                         ↓↑ Data Binding
┌─────────────────────────────────────────────────────┐
│                   ViewModels                        │
│  INotifyPropertyChanged, Commands, Validation      │
└─────────────────────────────────────────────────────┘
                         ↓↑
┌─────────────────────────────────────────────────────┐
│                    Services                         │
│  ConfigService | HotkeyService | ActionExecutor    │
└─────────────────────────────────────────────────────┘
                         ↓↑
┌─────────────────────────────────────────────────────┐
│                 Infrastructure                      │
│  Windows API | File System | Process Management    │
└─────────────────────────────────────────────────────┘
```

### 3.2 MVVMパターン

CommunityToolkit.Mvvm を使用してMVVMパターンを実装：

- **Model**: データ構造（Panel, Button, Action等）
- **View**: XAML画面（データバインディングのみ）
- **ViewModel**: UIロジック、コマンド、バリデーション

## 4. コンポーネント詳細設計

### 4.1 パネルウィンドウ (PanelWindow)

**責務**: オーバーレイ表示、ボタングリッド描画、ドラッグ移動

**特性**:
- `WindowStyle="None"` - タイトルバーなし
- `AllowsTransparency="True"` - 透過対応
- `Topmost="True"` - 常に最前面
- `ShowInTaskbar="False"` - タスクバー非表示

**ウィンドウ設定**:
```xml
<Window WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        Topmost="True"
        ShowInTaskbar="False"
        ResizeMode="NoResize">
```

### 4.2 グローバルホットキー (HotkeyService)

**実装方式**: `RegisterHotKey` / `UnregisterHotKey` API

```csharp
// P/Invoke定義
[DllImport("user32.dll")]
static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

[DllImport("user32.dll")]
static extern bool UnregisterHotKey(IntPtr hWnd, int id);
```

**ホットキー検出フロー**:
1. HwndSource でメッセージフック設定
2. WM_HOTKEY (0x0312) をリッスン
3. 登録済みホットキーIDとマッチング
4. 対応するアクションを発火

### 4.3 アクション実行エンジン (ActionExecutor)

**Strategy パターン** でアクションタイプごとに実装：

```csharp
public interface IActionHandler
{
    ActionType Type { get; }
    Task ExecuteAsync(ActionPayload payload);
}

public class HotkeyActionHandler : IActionHandler { }
public class CommandActionHandler : IActionHandler { }
public class OpenActionHandler : IActionHandler { }
public class TextActionHandler : IActionHandler { }
```

### 4.4 設定管理 (ConfigService)

**保存場所**: `%APPDATA%\HoverPad\config.json`

**機能**:
- JSON形式での設定読み書き
- 自動バックアップ（変更前に `.bak` 作成）
- スキーマバリデーション
- デフォルト値の適用

## 5. データフロー

### 5.1 アプリケーション起動

```
App.OnStartup
    ↓
ConfigService.LoadAsync()
    ↓
DI Container Setup
    ↓
HotkeyService.RegisterAll()
    ↓
SystemTray Icon Setup
    ↓
(各Panelの初期表示 if mode == "docked")
```

### 5.2 ホットキー → パネル表示

```
User: Ctrl+Shift+Space
    ↓
WndProc: WM_HOTKEY
    ↓
HotkeyService.OnHotkeyPressed(id)
    ↓
PanelViewModel.ShowCommand.Execute()
    ↓
PanelWindow.Show() at cursor position
```

### 5.3 ボタンクリック → アクション実行

```
User: Button Click
    ↓
Button.Command → ButtonViewModel.ExecuteCommand
    ↓
ActionExecutor.ExecuteAsync(actions[])
    ↓
foreach action:
    await handler.ExecuteAsync(payload)
    await Task.Delay(action.Delay)
    ↓
(Optional) PanelWindow.Hide()
```

## 6. セキュリティ考慮事項

1. **コマンド実行**: 信頼できるコマンドのみ実行
2. **ファイルアクセス**: 設定ファイルはユーザーディレクトリのみ
3. **入力検証**: 全ての入力値をサニタイズ

## 7. パフォーマンス最適化

1. **遅延読み込み**: パネルは必要時に初期化
2. **仮想化**: 大量ボタン時はVirtualizingStackPanel使用
3. **非同期処理**: I/O操作は全てasync/await
4. **メモリ管理**: WeakReferenceでイベントリーク防止

## 8. 配布設定

### Self-Contained Publish

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

**出力**: 単一の `HoverPad.exe`（約30-50MB）
- .NET Runtime同梱
- 追加インストール不要
- そのまま配布可能
