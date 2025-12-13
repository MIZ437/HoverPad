using System.Text.Json.Serialization;

namespace HoverPad.Models;

public class AppConfig
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("panels")]
    public List<Panel> Panels { get; set; } = new();

    [JsonPropertyName("profiles")]
    public List<Profile> Profiles { get; set; } = new();

    [JsonPropertyName("settings")]
    public AppSettings Settings { get; set; } = new();
}

public class AppSettings
{
    [JsonPropertyName("startWithOS")]
    public bool StartWithOS { get; set; } = false;

    [JsonPropertyName("showInTray")]
    public bool ShowInTray { get; set; } = true;

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";

    [JsonPropertyName("hideOnActionExecute")]
    public bool HideOnActionExecute { get; set; } = true;

    [JsonPropertyName("animationDurationMs")]
    public int AnimationDurationMs { get; set; } = 150;
}
