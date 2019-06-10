namespace Sanet.MagicalYatzy.Models.Game.Extensions
{
    public static class DiceStyleExtensions
    {

        public static string ToPathComponent(this DiceStyle style)
        {
            switch (style)
            {
                case DiceStyle.Red:
                    return "_1.";
                case DiceStyle.Blue:
                    return "_2.";
            }

            return "_0.";
        }
    }
}
