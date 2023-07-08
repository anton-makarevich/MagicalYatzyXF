namespace Sanet.MagicalYatzy.Models.Game.DiceGenerator;

public interface IValueGenerator
{
    int Next(int min, int max);
}