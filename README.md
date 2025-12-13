# HoverPad

デスクトップ画面上に表示される仮想ボタンパネル（オーバーレイ）アプリケーション

## 概要

HoverPadは、ユーザーが定義したアクションをワンクリックで実行できるワークスペースコントローラーです。

## 主な機能

- 画面上に常駐するカスタマイズ可能なボタンパネル
- 各ボタンに任意のアクション（コマンド実行、キー入力、アプリ起動等）を割り当て
- 固定表示 / ホットキー呼び出しの切替
- 複数パネルの同時運用
- プロファイル機能によるコンテキスト切替

## 技術スタック

- .NET 8 / WPF
- C# 12
- CommunityToolkit.Mvvm
- Hardcodet.NotifyIcon.Wpf

## プロジェクト構造

```
HoverPad/
├── src/
│   └── HoverPad/              # メインアプリケーション
│       ├── Models/            # データモデル
│       ├── Views/             # XAML画面
│       ├── ViewModels/        # ViewModel (MVVM)
│       ├── Services/          # ビジネスロジック
│       ├── Infrastructure/    # Windows API等
│       └── Converters/        # 値コンバーター
├── docs/                      # 設計ドキュメント
│   ├── architecture.md        # アーキテクチャ設計書
│   ├── data-models.md         # データモデル設計書
│   └── ui-design.md           # UI設計書
└── tests/                     # テスト
```

## ビルド方法

```bash
dotnet build
```

## 実行方法

```bash
dotnet run --project src/HoverPad
```

## 配布用ビルド

```bash
dotnet publish src/HoverPad -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## 使い方

1. アプリを起動するとシステムトレイにアイコンが表示されます
2. `Ctrl+Shift+Space` でパネルを表示/非表示を切替
3. パネルのボタンをクリックしてアクションを実行
4. トレイアイコンを右クリックして設定やパネル管理を行います

## 設定ファイル

設定は `%APPDATA%\HoverPad\config.json` に保存されます

## ライセンス

個人学習・プログラミング学習用
