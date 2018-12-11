using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Extensions
{
    public static class GameExtensions
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
