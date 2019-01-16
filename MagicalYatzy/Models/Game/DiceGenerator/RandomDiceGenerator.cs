using System;

namespace Sanet.MagicalYatzy.Models.Game.DiceGenerator
{
    public class RandomDiceGenerator: IDiceGenerator
    {
        private readonly Random _random = new Random();
        public int GetNextDiceResult(int[] previousResults = null)
        {
            return _random.Next(1,7);
        }
    }
}