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
        int _move;
        public int Move
        {
            get
            {
                return _move;
            }
        }

        public MoveEventArgs(IPlayer player, int move)
            : base(player)
        {
            _move = move;
        }
    }
    /// <summary>
    /// Event when player fix (unfix) dice with value
    /// </summary>
    public class FixDiceEventArgs : PlayerEventArgs
    {
        //dice with value to fix
        int _value;
        public int Value
        {
            get
            {
                return _value;
            }
        }
        //fix or unfix
        bool _isFixed;
        public bool Isfixed
        {
            get { return _isFixed; }
        }

        public FixDiceEventArgs(IPlayer player, int value, bool isfixed) : base(player)
        {
            _value = value;
            _isFixed = isfixed;
        }
    }

    /// <summary>
    /// Chat message event args
    /// </summary>
    public class ChatMessageEventArgs : EventArgs
    {
        private readonly ChatMessage _message;

        public ChatMessage Message { get { return _message; } }


        public ChatMessageEventArgs(ChatMessage message)
        {
            _message = message;
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
        IRollResult _result;
        public IRollResult Result
        {
            get
            {
                return _result;
            }
        }

        public ResultEventArgs(IPlayer player, IRollResult result)
            : base(player)
        {
            _result = result;
        }
    }

    public class StringEventArgs : EventArgs
    {
        private readonly string _str;
        public string Str { get { return _str; } }

        public StringEventArgs(string s)
        {
            _str = s;
        }
    }

    /// <summary>
    /// EventArgs avec un param d'un type quelconque (Key)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class KeyEventArgs<TKey> : EventArgs
    {
        private TKey _key;
        public TKey Key { get { return _key; } }

        public KeyEventArgs(TKey key)
        {
            _key = key;
        }
    }

    /// <summary>
    /// EventArgs avec un param d'un type quelconque (Key) et un autre param d'un type quelconque (Value)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeyValueEventArgs<TKey, TValue> : EventArgs
    {
        private TKey _key;
        private TValue _value;
        public TKey Key { get { return _key; } }
        public TValue Value { get { return _value; } }

        public KeyValueEventArgs(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }
    }

    /// <summary>
    /// EventArgs avec un param d'un type quelconque (Key) et d'autres params d'un type quelconque (Values)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeyValuesEventArgs<TKey, TValue> : EventArgs
    {
        private TKey _key;
        private TValue[] _values;
        public TKey Key { get { return _key; } }
        public TValue[] Values { get { return _values; } }

        public KeyValuesEventArgs(TKey key, params TValue[] values)
        {
            _key = key;
            _values = values;
        }
    }
}
