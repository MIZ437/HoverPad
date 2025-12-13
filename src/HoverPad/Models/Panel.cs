using System.Text.Json.Serialization;

namespace HoverPad.Models;

public class Panel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "新規パネル";

    [JsonPropertyName("rows")]
    public int Rows { get; set; } = 3;

    [JsonPropertyName("cols")]
    public int Cols { get; set; } = 4;

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PanelMode Mode { get; set; } = PanelMode.Dynamic;

    [JsonPropertyName("hotkey")]
    public string? Hotkey { get; set; }

    [JsonPropertyName("position")]
    public PanelPosition Position { get; set; } = new();

    [JsonPropertyName("appearance")]
    public PanelAppearance Appearance { get; set; } = new();

    [JsonPropertyName("buttons")]
    public List<PanelButton> Buttons { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PanelMode
{
    Docked,
    Dynamic
}

public class PanelPosition
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

public class PanelAppearance
{
    [JsonPropertyName("opacity")]
    public double Opacity { get; set; } = 0.95;

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = "#2C3E50";

    [JsonPropertyName("buttonSize")]
    public int ButtonSize { get; set; } = 64;

    [JsonPropertyName("gap")]
    public int Gap { get; set; } = 8;

    [JsonPropertyName("borderRadius")]
    public int BorderRadius { get; set; } = 8;
}
