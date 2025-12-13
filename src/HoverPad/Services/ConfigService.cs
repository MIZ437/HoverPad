using System.IO;
using System.Text.Json;
using HoverPad.Models;

namespace HoverPad.Services;

public class ConfigService : IConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppConfig Config { get; private set; } = new();

    public string ConfigPath { get; }

    public ConfigService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataPath, "HoverPad");
        Directory.CreateDirectory(configDir);
        ConfigPath = Path.Combine(configDir, "config.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(ConfigPath))
        {
            Config = CreateDefaultConfig();
            await SaveAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            Config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? CreateDefaultConfig();
        }
        catch (Exception)
        {
            Config = CreateDefaultConfig();
        }
    }

    public async Task SaveAsync()
    {
        CreateBackup();
        var json = JsonSerializer.Serialize(Config, JsonOptions);
        await File.WriteAllTextAsync(ConfigPath, json);
    }

    public void CreateBackup()
    {
        if (File.Exists(ConfigPath))
        {
            var backupPath = ConfigPath + ".bak";
            File.Copy(ConfigPath, backupPath, overwrite: true);
        }
    }

    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            Version = "1.0",
            Panels = new List<Panel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "メインパネル",
                    Rows = 3,
                    Cols = 4,
                    Mode = PanelMode.Dynamic,
                    Hotkey = "Ctrl+Shift+Space",
                    Buttons = new List<PanelButton>
                    {
                        new()
                        {
                            Position = new GridPosition { Row = 0, Col = 0 },
                            Label = "メモ帳",
                            Icon = "\U0001F4DD",
                            Actions = new List<ButtonAction>
                            {
                                new()
                                {
                                    Type = ActionType.Open,
                                    Payload = JsonSerializer.SerializeToElement(new OpenPayload { Path = "notepad.exe" })
                                }
                            }
                        },
                        new()
                        {
                            Position = new GridPosition { Row = 0, Col = 1 },
                            Label = "コピー",
                            Icon = "\U0001F4CB",
                            Actions = new List<ButtonAction>
                            {
                                new()
                                {
                                    Type = ActionType.Hotkey,
                                    Payload = JsonSerializer.SerializeToElement(new HotkeyPayload { Keys = "Ctrl+C" })
                                }
                            }
                        },
                        new()
                        {
                            Position = new GridPosition { Row = 0, Col = 2 },
                            Label = "貼り付け",
                            Icon = "\U0001F4C4",
                            Actions = new List<ButtonAction>
                            {
                                new()
                                {
                                    Type = ActionType.Hotkey,
                                    Payload = JsonSerializer.SerializeToElement(new HotkeyPayload { Keys = "Ctrl+V" })
                                }
                            }
                        }
                    }
                }
            },
            Profiles = new List<Profile>
            {
                new()
                {
                    Name = "デフォルト",
                    IsDefault = true
                }
            },
            Settings = new AppSettings()
        };
    }
}
