using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoverPad.Models;

public class PanelButton
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("position")]
    public GridPosition Position { get; set; } = new();

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("actions")]
    public List<ButtonAction> Actions { get; set; } = new();

    [JsonPropertyName("style")]
    public ButtonStyle? Style { get; set; }

    [JsonPropertyName("isToggle")]
    public bool IsToggle { get; set; }

    [JsonPropertyName("toggleState")]
    public bool ToggleState { get; set; }
}

public class GridPosition
{
    [JsonPropertyName("row")]
    public int Row { get; set; }

    [JsonPropertyName("col")]
    public int Col { get; set; }
}

public class ButtonStyle
{
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("foregroundColor")]
    public string? ForegroundColor { get; set; }

    [JsonPropertyName("borderColor")]
    public string? BorderColor { get; set; }

    [JsonPropertyName("borderThickness")]
    public int? BorderThickness { get; set; }
}

public class ButtonAction
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActionType Type { get; set; }

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    [JsonPropertyName("delayMs")]
    public int DelayMs { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionType
{
    Hotkey,
    Text,
    Open,
    Command,
    Folder,
    ProfileSwitch,
    System,
    MultiAction
}
