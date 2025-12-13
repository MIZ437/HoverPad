using System.Windows.Input;
using System.Windows.Interop;
using HoverPad.Infrastructure;

namespace HoverPad.Services;

public class HotkeyService : IHotkeyService
{
    private nint _windowHandle;
    private HwndSource? _source;
    private readonly Dictionary<int, string> _registeredHotkeys = new();

    public event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    public void Initialize(nint windowHandle)
    {
        _windowHandle = windowHandle;
        _source = HwndSource.FromHwnd(windowHandle);
        _source?.AddHook(WndProc);
    }

    public bool Register(int id, string hotkey)
    {
        if (_windowHandle == nint.Zero) return false;

        var (modifiers, vk) = ParseHotkey(hotkey);
        if (vk == 0) return false;

        var result = NativeMethods.RegisterHotKey(_windowHandle, id, (uint)modifiers, vk);
        if (result)
        {
            _registeredHotkeys[id] = hotkey;
        }
        return result;
    }

    public bool Unregister(int id)
    {
        if (_windowHandle == nint.Zero) return false;

        var result = NativeMethods.UnregisterHotKey(_windowHandle, id);
        if (result)
        {
            _registeredHotkeys.Remove(id);
        }
        return result;
    }

    public void UnregisterAll()
    {
        foreach (var id in _registeredHotkeys.Keys.ToList())
        {
            Unregister(id);
        }
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            if (_registeredHotkeys.TryGetValue(id, out var hotkey))
            {
                HotkeyPressed?.Invoke(this, new HotkeyEventArgs(id, hotkey));
                handled = true;
            }
        }
        return nint.Zero;
    }

    private static (NativeMethods.ModifierKeys modifiers, uint vk) ParseHotkey(string hotkey)
    {
        var modifiers = NativeMethods.ModifierKeys.None;
        uint vk = 0;

        var parts = hotkey.Split('+').Select(p => p.Trim()).ToArray();
        foreach (var part in parts)
        {
            switch (part.ToLower())
            {
                case "ctrl":
                case "control":
                    modifiers |= NativeMethods.ModifierKeys.Control;
                    break;
                case "alt":
                    modifiers |= NativeMethods.ModifierKeys.Alt;
                    break;
                case "shift":
                    modifiers |= NativeMethods.ModifierKeys.Shift;
                    break;
                case "win":
                case "windows":
                    modifiers |= NativeMethods.ModifierKeys.Win;
                    break;
                case "space":
                    vk = 0x20;
                    break;
                default:
                    if (part.Length == 1)
                    {
                        vk = (uint)KeyInterop.VirtualKeyFromKey(
                            (Key)Enum.Parse(typeof(Key), part.ToUpper()));
                    }
                    else if (Enum.TryParse<Key>(part, true, out var key))
                    {
                        vk = (uint)KeyInterop.VirtualKeyFromKey(key);
                    }
                    break;
            }
        }

        return (modifiers, vk);
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
        _source = null;
    }
}
