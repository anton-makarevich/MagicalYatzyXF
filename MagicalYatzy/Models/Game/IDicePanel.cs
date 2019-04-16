using System;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Models.Events;
using static Sanet.MagicalYatzy.Models.Events.GameEvents;

namespace Sanet.MagicalYatzy.Models.Game
{
    public interface IDicePanel: IDisposable
    {
        bool AreAllDiceStopped { get; }
        bool ClickToFix { get; set; }
        bool DebugDrawMode { get; set; }
        int DiceCount { get; set; }
        bool ManualSetMode { get; set; }
        bool IsRolling { get; }
        DiceStyle PanelStyle { get; }
        bool PlaySound { get; set; }
        DieResult Result { get; }
        int RollDelay { get; set; }
        bool TreeDScale { get; set; }
        double TreeDScaleFactor { get; set; }
        bool WithSound { get; set; }
        Rectangle Bounds { get; }
        int FixedDiceCount { get; }
        bool AreAllDiceFixed { get; }

        event DieChangedEventHandler DieChangedManually;
        event EventHandler<DiceFixedEventArgs> DieFixed;
        event DieManualChangeRequestEventHandler DieManualChangeRequested;

        event EventHandler RollEnded;
        event EventHandler RollStarted;
        event EventHandler<Die> DieAdded;
        event EventHandler<Die> DieRemoved;

        void ChangeDice(int oldValue, int newValue);
        void UnfixAll();
        void DieClicked(Point clickLocation);
        void FixDice(int index, bool isFixed);
        bool RollDice(List<int> aResults);
        void Resize(int width, int height);
        void ChangeDiceManually(int newValue);

        Point? GetDicePosition(int diceValue);
    }
}