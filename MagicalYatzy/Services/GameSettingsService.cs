using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services
{
    public class GameSettingsService : IGameSettingsService
    {
        private int _dieAngle = 2;
        private int _maxRollLoop = 100;
        private int _dieSpeed = (int)DiceSpeed.Fast;

        public DiceStyle DieStyle { get; set; }
        public int DieAngle {
            get => _dieAngle;
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 5)
                    value = 5;
                _dieAngle = value;
            }
        }
        public int MaxRollLoop {
            get => _maxRollLoop;
            set
            {
                if (value < 20)
                    value = 20;
                if (value > 150)
                    value = 150;
                _maxRollLoop = value;
            }
        }

        public int DieSpeed
        {
            get => _dieSpeed;
            set
            {
                _dieSpeed = value switch
                {
                    < 15 => 15,
                    > 70 => 70,
                    _ => value
                };
            }
        }

        public bool IsSoundEnabled { get; set; }
    }
}
