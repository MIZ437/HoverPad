using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using HoverPad.Infrastructure;
using HoverPad.Models;

namespace HoverPad.Services;

public class ActionExecutor : IActionExecutor
{
    public async Task ExecuteAsync(IEnumerable<ButtonAction> actions)
    {
        foreach (var action in actions)
        {
            await ExecuteAsync(action);
            if (action.DelayMs > 0)
            {
                await Task.Delay(action.DelayMs);
            }
        }
    }

    public async Task ExecuteAsync(ButtonAction action)
    {
        switch (action.Type)
        {
            case ActionType.Hotkey:
                await ExecuteHotkeyAsync(action);
                break;
            case ActionType.Text:
                await ExecuteTextAsync(action);
                break;
            case ActionType.Open:
                await ExecuteOpenAsync(action);
                break;
            case ActionType.Command:
                await ExecuteCommandAsync(action);
                break;
            case ActionType.System:
                await ExecuteSystemAsync(action);
                break;
        }
    }

    private Task ExecuteHotkeyAsync(ButtonAction action)
    {
        if (action.Payload == null) return Task.CompletedTask;

        var payload = JsonSerializer.Deserialize<HotkeyPayload>(action.Payload.Value.GetRawText());
        if (payload == null) return Task.CompletedTask;

        SendKeys(payload.Keys);
        return Task.CompletedTask;
    }

    private static void SendKeys(string keys)
    {
        var modifiersToRelease = new List<byte>();
        var keyToPress = 0;

        var parts = keys.Split('+').Select(p => p.Trim()).ToArray();
        foreach (var part in parts)
        {
            switch (part.ToLower())
            {
                case "ctrl":
                case "control":
                    NativeMethods.keybd_event(NativeMethods.VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYDOWN, 0);
                    modifiersToRelease.Add(NativeMethods.VK_CONTROL);
                    break;
                case "alt":
                    NativeMethods.keybd_event(NativeMethods.VK_MENU, 0, NativeMethods.KEYEVENTF_KEYDOWN, 0);
                    modifiersToRelease.Add(NativeMethods.VK_MENU);
                    break;
                case "shift":
                    NativeMethods.keybd_event(NativeMethods.VK_SHIFT, 0, NativeMethods.KEYEVENTF_KEYDOWN, 0);
                    modifiersToRelease.Add(NativeMethods.VK_SHIFT);
                    break;
                case "win":
                case "windows":
                    NativeMethods.keybd_event(NativeMethods.VK_LWIN, 0, NativeMethods.KEYEVENTF_KEYDOWN, 0);
                    modifiersToRelease.Add(NativeMethods.VK_LWIN);
                    break;
                default:
                    if (part.Length == 1)
                    {
                        keyToPress = KeyInterop.VirtualKeyFromKey(
                            (Key)Enum.Parse(typeof(Key), part.ToUpper()));
                    }
                    else if (Enum.TryParse<Key>(part, true, out var key))
                    {
                        keyToPress = KeyInterop.VirtualKeyFromKey(key);
                    }
                    break;
            }
        }

        if (keyToPress > 0)
        {
            NativeMethods.keybd_event((byte)keyToPress, 0, NativeMethods.KEYEVENTF_KEYDOWN, 0);
            NativeMethods.keybd_event((byte)keyToPress, 0, NativeMethods.KEYEVENTF_KEYUP, 0);
        }

        foreach (var modifier in modifiersToRelease)
        {
            NativeMethods.keybd_event(modifier, 0, NativeMethods.KEYEVENTF_KEYUP, 0);
        }
    }

    private Task ExecuteTextAsync(ButtonAction action)
    {
        if (action.Payload == null) return Task.CompletedTask;

        var payload = JsonSerializer.Deserialize<TextPayload>(action.Payload.Value.GetRawText());
        if (payload == null) return Task.CompletedTask;

        if (payload.Mode == TextInputMode.Clipboard)
        {
            Clipboard.SetText(payload.Text);
            SendKeys("Ctrl+V");
        }
        else
        {
            System.Windows.Forms.SendKeys.SendWait(payload.Text);
        }

        return Task.CompletedTask;
    }

    private Task ExecuteOpenAsync(ButtonAction action)
    {
        if (action.Payload == null) return Task.CompletedTask;

        var payload = JsonSerializer.Deserialize<OpenPayload>(action.Payload.Value.GetRawText());
        if (payload == null) return Task.CompletedTask;

        var startInfo = new ProcessStartInfo
        {
            FileName = payload.Path,
            Arguments = payload.Arguments ?? "",
            WorkingDirectory = payload.WorkingDirectory ?? "",
            UseShellExecute = true
        };

        if (payload.RunAsAdmin)
        {
            startInfo.Verb = "runas";
        }

        Process.Start(startInfo);
        return Task.CompletedTask;
    }

    private async Task ExecuteCommandAsync(ButtonAction action)
    {
        if (action.Payload == null) return;

        var payload = JsonSerializer.Deserialize<CommandPayload>(action.Payload.Value.GetRawText());
        if (payload == null) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {payload.Command}",
            UseShellExecute = false,
            CreateNoWindow = !payload.ShowWindow
        };

        var process = Process.Start(startInfo);
        if (process != null && payload.WaitForExit)
        {
            await process.WaitForExitAsync();
        }
    }

    private Task ExecuteSystemAsync(ButtonAction action)
    {
        if (action.Payload == null) return Task.CompletedTask;

        var payload = JsonSerializer.Deserialize<SystemPayload>(action.Payload.Value.GetRawText());
        if (payload == null) return Task.CompletedTask;

        switch (payload.Action)
        {
            case SystemAction.VolumeUp:
                SendKeys("VolumeUp");
                break;
            case SystemAction.VolumeDown:
                SendKeys("VolumeDown");
                break;
            case SystemAction.VolumeMute:
                SendKeys("VolumeMute");
                break;
            case SystemAction.MediaPlayPause:
                SendKeys("MediaPlayPause");
                break;
            case SystemAction.MediaNext:
                SendKeys("MediaNextTrack");
                break;
            case SystemAction.MediaPrevious:
                SendKeys("MediaPreviousTrack");
                break;
        }

        return Task.CompletedTask;
    }
}
