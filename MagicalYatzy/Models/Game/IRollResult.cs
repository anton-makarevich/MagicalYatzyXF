namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IRollResult
    {
        bool HasBonus { get; set; }
        bool HasValue { get; set; }
        bool IsNumeric { get; }
        bool IsZeroValue { get; }
        int MaxValue { get; }
        int PossibleValue { get; set; }
        Scores ScoreType { get; set; }

        int Value { get; set; }
    }
}
