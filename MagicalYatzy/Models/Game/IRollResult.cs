namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IRollResult
    {
        bool HasBonus { get; set; }
        bool HasValue { get; }
        bool IsMaxPossibleValue { get; }
        bool IsNumeric { get; }
        bool IsZeroValue { get; }
        int MaxValue { get; }
        int PossibleValue { get; set; }
        Scores ScoreType { get; }
        int Value { get; set; }
        ScoreStatus Status { get; }
    }
}