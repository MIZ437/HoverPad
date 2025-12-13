using System.Text.Json.Serialization;

namespace HoverPad.Models;

public class Profile
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "新規プロファイル";

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("autoSwitch")]
    public List<AutoSwitchRule> AutoSwitch { get; set; } = new();
}

public class AutoSwitchRule
{
    [JsonPropertyName("processName")]
    public string? ProcessName { get; set; }

    [JsonPropertyName("windowTitle")]
    public string? WindowTitle { get; set; }
}
