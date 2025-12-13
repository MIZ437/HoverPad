using System.Text.Json.Serialization;

namespace HoverPad.Models;

public class HotkeyPayload
{
    [JsonPropertyName("keys")]
    public string Keys { get; set; } = "";
}

public class TextPayload
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TextInputMode Mode { get; set; } = TextInputMode.Type;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TextInputMode
{
    Type,
    Clipboard
}

public class OpenPayload
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    [JsonPropertyName("workingDirectory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName("runAsAdmin")]
    public bool RunAsAdmin { get; set; }
}

public class CommandPayload
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = "";

    [JsonPropertyName("showWindow")]
    public bool ShowWindow { get; set; }

    [JsonPropertyName("waitForExit")]
    public bool WaitForExit { get; set; }
}

public class SystemPayload
{
    [JsonPropertyName("action")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SystemAction Action { get; set; }

    [JsonPropertyName("value")]
    public int? Value { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
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
