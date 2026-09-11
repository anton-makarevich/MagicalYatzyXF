namespace Sanet.MagicalYatzy.Models.Game.Ai
{
    public interface IGameDecisionMaker
    {
        bool NeedsToRollAgain();
        void FixDice(IGame game);
        void DecideFill(IGame game);
        void DecideRoll(IGame game, IDicePanel dicePanel);
    }
}