using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Game;
using System;

namespace Sanet.MagicalYatzy.Models.Events
{
    /// <summary>
    /// Event arg when player changed
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        //Current player
        private IPlayer _player;
        public IPlayer Player { get { return _player; } }

        public PlayerEventArgs(IPlayer player)
        {
            _player = player;
        }
    }
    
    /// <summary>
    /// Event when move changed
    /// </summary>
    public class MoveEventArgs : PlayerEventArgs
    {
        //new move order
        public int Move { get; }

        public MoveEventArgs(IPlayer player, int move)
            : base(player)
        {
            Move = move;
        }
    }
    
    /// <summary>
    /// Event when player fix (unfix) dice with value
    /// </summary>
    public class FixDiceEventArgs : PlayerEventArgs
    {
        //dice with value to fix
        public int Value { get; }

        //fix or unfix
        public bool Isfixed { get; }

        public FixDiceEventArgs(IPlayer player, int value, bool isfixed) : base(player)
        {
            Value = value;
            Isfixed = isfixed;
        }
    }

    /// <summary>
    /// Chat message event args
    /// </summary>
    public class ChatMessageEventArgs : EventArgs
    {
        public ChatMessage Message { get; }

        public ChatMessageEventArgs(ChatMessage message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Event when move changed
    /// </summary>
    public class RollEventArgs : PlayerEventArgs
    {
        //new move order
        int[] _value;
        public int[] Value
        {
            get
            {
                return _value;
            }
        }

        public RollEventArgs(IPlayer player, int[] value)
            : base(player)
        {
            _value = value;
        }
    }
    
    /// <summary>
    /// Event to notified about score applied
    /// </summary>
    public class ResultEventArgs : PlayerEventArgs
    {
        //new move order
        public IRollResult Result { get; }

        public ResultEventArgs(IPlayer player, IRollResult result)
            : base(player)
        {
            Result = result;
        }
    }
}
