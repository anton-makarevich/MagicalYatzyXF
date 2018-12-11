namespace Sanet.MagicalYatzy.Models.Events
{
    public static class GameEvents
    {
        public delegate void DieFrozenEventHandler(bool isFixed, int value);
        public delegate void DieChangedEventHandler(bool isFixed, int oldValue, int newValue);
    }
}
