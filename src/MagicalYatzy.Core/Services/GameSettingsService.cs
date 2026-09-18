using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services
{
    public class GameSettingsService : IGameSettingsService
    {
        public DiceStyle DieStyle { get; set; }

        public int DieAngle
        {
            get;
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 5)
                    value = 5;
                field = value;
            }
        } = 2;

        public int MaxRollLoop
        {
            get;
            set
            {
                if (value < 20)
                    value = 20;
                if (value > 150)
                    value = 150;
                field = value;
            }
        } = 100;

        public int DieSpeed
        {
            get;
            set
            {
                field = value switch
                {
                    < (int)DiceSpeed.VeryFast => (int)DiceSpeed.VeryFast,
                    > (int)DiceSpeed.VerySlow => (int)DiceSpeed.VerySlow,
                    _ => value
                };
            }
        } = (int)DiceSpeed.Fast;

        public bool IsSoundEnabled { get; set; }
    }
}
