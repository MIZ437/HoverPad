using HoverPad.Models;

namespace HoverPad.Services;

public interface IConfigService
{
    AppConfig Config { get; }
    Task LoadAsync();
    Task SaveAsync();
    void CreateBackup();
    string ConfigPath { get; }
}
