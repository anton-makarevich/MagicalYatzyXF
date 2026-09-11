using System;

namespace Sanet.MagicalYatzy.Models.Game.DiceGenerator;

public class RandomValueGenerator:IValueGenerator
{
    private readonly Random _random = new();
    public int Next(int min, int max)
    { 
        return _random.Next(min,max);
    }
}