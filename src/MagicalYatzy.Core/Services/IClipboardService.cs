using System.Threading.Tasks;

namespace Sanet.MagicalYatzy.Services;

public interface IClipboardService
{
    Task SetTextAsync(string text);
}