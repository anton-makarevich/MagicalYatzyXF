using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services
{
    public interface IGameSettingsService
    {
        int DieAngle { get; set; }
        DiceStyle DieStyle { get; set; }
        int MaxRollLoop { get; set; }
    }
}