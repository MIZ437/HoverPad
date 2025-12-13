# HoverPad データモデル設計書

## 1. クラス図

```
┌─────────────────────────────────────────────────────────────────┐
│                         AppConfig                                │
├─────────────────────────────────────────────────────────────────┤
│ + Version: string                                                │
│ + Panels: List<Panel>                                           │
│ + Profiles: List<Profile>                                       │
│ + Settings: AppSettings                                         │
└─────────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│     Panel       │  │    Profile      │  │  AppSettings    │
├─────────────────┤  ├─────────────────┤  ├─────────────────┤
│ + Id: Guid      │  │ + Id: Guid      │  │ + StartWithOS   │
│ + Name: string  │  │ + Name: string  │  │ + ShowInTray    │
│ + Rows: int     │  │ + IsDefault     │  │ + Theme: string │
│ + Cols: int     │  │ + AutoSwitch[]  │  └─────────────────┘
│ + Mode: enum    │  └─────────────────┘
│ + Hotkey        │
│ + Position      │
│ + Appearance    │
│ + Buttons[]     │
└─────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│              PanelButton                 │
├─────────────────────────────────────────┤
│ + Id: Guid                               │
│ + Position: GridPosition                 │
│ + Label: string                          │
│ + Icon: string                           │
│ + Actions: List<ButtonAction>           │
│ + Style: ButtonStyle                     │
└─────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│            ButtonAction                  │
├─────────────────────────────────────────┤
│ + Type: ActionType                       │
│ + Payload: object                        │
│ + DelayMs: int                           │
└─────────────────────────────────────────┘
```

## 2. モデル詳細

### 2.1 AppConfig（設定ルート）

```csharp
public class AppConfig
{
    public string Version { get; set; } = "1.0";
    public List<Panel> Panels { get; set; } = new();
    public List<Profile> Profiles { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}
```

### 2.2 Panel（パネル定義）

```csharp
public class Panel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "新規パネル";
    public int Rows { get; set; } = 3;
    public int Cols { get; set; } = 4;
    public PanelMode Mode { get; set; } = PanelMode.Dynamic;
    public string? Hotkey { get; set; }
    public PanelPosition Position { get; set; } = new();
    public PanelAppearance Appearance { get; set; } = new();
    public List<PanelButton> Buttons { get; set; } = new();
}

public enum PanelMode
{
    Docked,   // 固定表示
    Dynamic   // ホットキーで呼び出し
}
```

### 2.3 PanelPosition（位置情報）

```csharp
public class PanelPosition
{
    public double X { get; set; }
    public double Y { get; set; }
}
```

### 2.4 PanelAppearance（外観設定）

```csharp
public class PanelAppearance
{
    public double Opacity { get; set; } = 0.95;
    public string BackgroundColor { get; set; } = "#2C3E50";
    public int ButtonSize { get; set; } = 64;
    public int Gap { get; set; } = 8;
    public int BorderRadius { get; set; } = 8;
}
```

### 2.5 PanelButton（ボタン定義）

```csharp
public class PanelButton
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public GridPosition Position { get; set; } = new();
    public string Label { get; set; } = "";
    public string? Icon { get; set; }
    public List<ButtonAction> Actions { get; set; } = new();
    public ButtonStyle? Style { get; set; }
    public bool IsToggle { get; set; }
    public bool ToggleState { get; set; }
}

public class GridPosition
{
    public int Row { get; set; }
    public int Col { get; set; }
}

public class ButtonStyle
{
    public string? BackgroundColor { get; set; }
    public string? ForegroundColor { get; set; }
    public string? BorderColor { get; set; }
    public int? BorderThickness { get; set; }
}
```

### 2.6 ButtonAction（アクション定義）

```csharp
public class ButtonAction
{
    public ActionType Type { get; set; }
    public JsonElement? Payload { get; set; }
    public int DelayMs { get; set; }
}

public enum ActionType
{
    Hotkey,         // キーボードショートカット送信
    Text,           // テキスト入力
    Open,           // アプリ/URL/フォルダを開く
    Command,        // シェルコマンド実行
    Folder,         // サブパネルを開く
    ProfileSwitch,  // プロファイル切替
    System,         // システム制御（音量等）
    MultiAction     // 複数アクション
}
```

### 2.7 アクションペイロード型

```csharp
// ホットキー送信
public class HotkeyPayload
{
    public string Keys { get; set; } = "";
    // 例: "Ctrl+C", "Alt+Tab", "Win+D"
}

// テキスト入力
public class TextPayload
{
    public string Text { get; set; } = "";
    public TextInputMode Mode { get; set; } = TextInputMode.Type;
}

public enum TextInputMode
{
    Type,       // キーストロークで入力
    Clipboard   // クリップボード経由で貼り付け
}

// アプリ/URL/フォルダを開く
public class OpenPayload
{
    public string Path { get; set; } = "";
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
    public bool RunAsAdmin { get; set; }
}

// シェルコマンド実行
public class CommandPayload
{
    public string Command { get; set; } = "";
    public bool ShowWindow { get; set; }
    public bool WaitForExit { get; set; }
}

// システム制御
public class SystemPayload
{
    public SystemAction Action { get; set; }
    public int? Value { get; set; }
}

public enum SystemAction
{
    VolumeUp,
    VolumeDown,
    VolumeMute,
    MediaPlayPause,
    MediaNext,
    MediaPrevious,
    BrightnessUp,
    BrightnessDown
}
```

### 2.8 Profile（プロファイル定義）

```csharp
public class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "新規プロファイル";
    public bool IsDefault { get; set; }
    public List<AutoSwitchRule> AutoSwitch { get; set; } = new();
}

public class AutoSwitchRule
{
    public string? ProcessName { get; set; }
    public string? WindowTitle { get; set; }
}
```

### 2.9 AppSettings（アプリ設定）

```csharp
public class AppSettings
{
    public bool StartWithOS { get; set; } = false;
    public bool ShowInTray { get; set; } = true;
    public string Theme { get; set; } = "dark";
    public bool HideOnActionExecute { get; set; } = true;
    public int AnimationDurationMs { get; set; } = 150;
}
```

## 3. 設定ファイルサンプル (config.json)

```json
{
  "version": "1.0",
  "panels": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "メインパネル",
      "rows": 3,
      "cols": 4,
      "mode": "Dynamic",
      "hotkey": "Ctrl+Shift+Space",
      "position": { "x": 100, "y": 100 },
      "appearance": {
        "opacity": 0.95,
        "backgroundColor": "#2C3E50",
        "buttonSize": 64,
        "gap": 8,
        "borderRadius": 8
      },
      "buttons": [
        {
          "id": "btn-001",
          "position": { "row": 0, "col": 0 },
          "label": "メモ帳",
          "icon": "📝",
          "actions": [
            {
              "type": "Open",
              "payload": { "path": "notepad.exe" },
              "delayMs": 0
            }
          ],
          "style": {
            "backgroundColor": "#3498DB"
          }
        },
        {
          "id": "btn-002",
          "position": { "row": 0, "col": 1 },
          "label": "コピー",
          "icon": "📋",
          "actions": [
            {
              "type": "Hotkey",
              "payload": { "keys": "Ctrl+C" },
              "delayMs": 0
            }
          ]
        }
      ]
    }
  ],
  "profiles": [
    {
      "id": "profile-default",
      "name": "デフォルト",
      "isDefault": true,
      "autoSwitch": []
    }
  ],
  "settings": {
    "startWithOS": true,
    "showInTray": true,
    "theme": "dark",
    "hideOnActionExecute": true,
    "animationDurationMs": 150
  }
}
```

## 4. バリデーションルール

| フィールド | ルール |
|-----------|--------|
| Panel.Rows | 1-8 |
| Panel.Cols | 1-8 |
| Panel.Name | 1-50文字 |
| Appearance.Opacity | 0.0-1.0 |
| Appearance.ButtonSize | 32-256 |
| Appearance.Gap | 0-20 |
| Button.Label | 0-20文字 |
| Action.DelayMs | 0-60000 |
| Hotkey | 正規表現でパターンマッチ |

## 5. マイグレーション

バージョンアップ時の設定移行をサポート：

```csharp
public interface IConfigMigrator
{
    string FromVersion { get; }
    string ToVersion { get; }
    AppConfig Migrate(AppConfig oldConfig);
}
```
