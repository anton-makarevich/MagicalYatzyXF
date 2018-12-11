using Sanet.MagicalYatzy.Models.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanet.MagicalYatzy.Services
{
    public class GameSettingsService : IGameSettingsService
    {
        private int _dieAngle = 2;
        private int _maxRollLoop = 100;

        public DiceStyle DieStyle { get; set; }
        public int DieAngle {
            get
            {
                return _dieAngle;
            }
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
            get { return _maxRollLoop; }
            set
            {
                if (value < 20)
                    value = 20;
                if (value > 150)
                    value = 150;
                _maxRollLoop = value;
            }
        } 
    }
}
