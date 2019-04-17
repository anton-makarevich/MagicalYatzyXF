namespace Sanet.MagicalYatzy.Models.Game.Extensions
{
    public static class ScoreHelpers
    {
        public static bool IsNumeric(this Scores score)
        {
            return (score == Scores.Ones
                    || score == Scores.Twos
                    || score == Scores.Threes
                    || score == Scores.Fours
                    || score == Scores.Fives
                    || score == Scores.Sixs);
        }

        public static int GetMaxValue(this Scores score) 
        {
            switch (score)
            {
                case Scores.Ones:
                    return 5;
                case Scores.Twos:
                    return 10;
                case Scores.Threes:
                    return 15;
                case Scores.Fours:
                    return 20;
                case Scores.Fives:
                    return 25;
                case Scores.Sixs:
                case Scores.ThreeOfAKind:
                case Scores.SmallStraight:
                case Scores.FourOfAKind:
                case Scores.Chance:
                    return 30;
                case Scores.Bonus:
                    return 35;
                case Scores.FullHouse:
                    return 25;
                case Scores.LargeStraight:
                    return 40;
                case Scores.Kniffel:
                    return 50;
            }

            return 0;
        }
    }
}
