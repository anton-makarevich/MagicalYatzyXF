namespace Sanet.MagicalYatzy.Models.Game.DiceGenerator
{
    public interface IDiceGenerator
    {
        int GetNextDiceResult(int[] previousResults = null);
    }
}