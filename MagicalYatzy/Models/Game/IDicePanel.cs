using System;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Common;
using static Sanet.MagicalYatzy.Models.Events.GameEvents;

namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IDicePanel
    {
        bool AllDiceStopped { get; }
        bool ClickToFreeze { get; set; }
        bool DebugDrawMode { get; set; }
        int DiceCount { get; set; }
        bool ManualSetMode { get; set; }

        DiceStyle PanelStyle { get; }
        bool PlaySound { get; set; }
        DieResult Result { get; }
        int RollDelay { get; set; }
        bool TreeDScale { get; set; }
        double TreeDScaleCoef { get; set; }
        bool WithSound { get; set; }
        Rectangle Bounds { get; }

        event DieChangedEventHandler DieChangedManually;
        event DieFrozenEventHandler DieFrozen;
        event Action RollEnded;
        event Action RollStarted;
        event Action<Die> DieAdded;
        event Action<Die> DieRemoved;

        bool AllDiceFrozen();
        void ChangeDice(int oldValue, int newValue);
        void ClearFreeze();
        void DieClicked(object sender, IEnumerable<Point> e);
        void Dispose();
        void FixDice(int index, bool isfixed);
        int FrozenCount();
        bool RollDice(List<int> aResults);
        void Resize(int width, int height);
    }
}