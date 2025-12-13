using HoverPad.Models;

namespace HoverPad.Services;

public interface IActionExecutor
{
    Task ExecuteAsync(IEnumerable<ButtonAction> actions);
    Task ExecuteAsync(ButtonAction action);
}
