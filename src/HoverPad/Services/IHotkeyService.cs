namespace HoverPad.Services;

public interface IHotkeyService : IDisposable
{
    event EventHandler<HotkeyEventArgs>? HotkeyPressed;
    bool Register(int id, string hotkey);
    bool Unregister(int id);
    void UnregisterAll();
    void Initialize(nint windowHandle);
}

public class HotkeyEventArgs : EventArgs
{
    public int Id { get; }
    public string Hotkey { get; }

    public HotkeyEventArgs(int id, string hotkey)
    {
        Id = id;
        Hotkey = hotkey;
    }
}
