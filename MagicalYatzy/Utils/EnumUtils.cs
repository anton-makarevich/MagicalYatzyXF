using System;
namespace Sanet.MagicalYatzy.Utils
{
    public static class EnumUtils
    {
        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
