using System;
using System.Linq;

namespace Sanet.MagicalYatzy.Models.Game.DiceGenerator;

public class AllOfKindTestGenerator : IDiceGenerator
{
    private readonly Random _random = new Random();
    public int GetNextDiceResult(int[] previousResults = null)
    {
        if (previousResults != null && previousResults.Length > 0)
            return previousResults.First();
        return _random.Next(1, 7);
    }
}