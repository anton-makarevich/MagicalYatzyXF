using System;

namespace Sanet.MagicalYatzy.Models.Events
{
    public class DiceFixedEventArgs:EventArgs
    {
        public bool IsFixed { get; }
        public int Value { get; }
        
        public DiceFixedEventArgs(bool isFixed, int value)
        {
            IsFixed = isFixed;
            Value = value;
        }
    }
}