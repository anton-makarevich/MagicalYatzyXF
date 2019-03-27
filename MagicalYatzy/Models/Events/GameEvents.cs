using System;

namespace Sanet.MagicalYatzy.Models.Events
{
    public static class GameEvents
    {
        public delegate void DieChangedEventHandler(bool isFixed, int oldValue, int newValue);
        public delegate void DieManualChangeRequestEventHandler(Action<int> updateValue);
    }
}
