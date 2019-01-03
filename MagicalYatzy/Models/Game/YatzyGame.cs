using System;
using System.Linq;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class YatzyGame: IGame
    {
        private bool _isPlaying;

        #region Events

        public YatzyGame(Rules rules)
        {
            Rules = new Rule(rules);
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString("N");
        }

        public YatzyGame():this(Game.Rules.krExtended)
        {
        }

        public event EventHandler<PlayerEventArgs> PlayerLeft;
        public event EventHandler<RollEventArgs> DiceChanged;
        public event EventHandler<FixDiceEventArgs> DiceFixed;
        public event EventHandler<RollEventArgs> DiceRolled;
        public event EventHandler<PlayerEventArgs> PlayerReady;
        public event EventHandler<PlayerEventArgs> PlayerRerolled;
        public event EventHandler<PlayerEventArgs> MagicRollUsed;
        public event EventHandler<PlayerEventArgs> StyleChanged;
        
        public event EventHandler GameFinished;
        
        public event EventHandler<MoveEventArgs> MoveChanged;
        public event EventHandler<ChatMessageEventArgs> OnChatMessage;
        
        public event EventHandler<PlayerEventArgs> PlayerJoined;
        
        public event EventHandler<ResultEventArgs> ResultApplied;
        #endregion

        #region Properties

        public IPlayer CurrentPlayer { get; private set; } 
        public string GameId { get; }
        public int FixedDicesCount { get; }

        public bool IsPlaying
        {
            get
            {
                if (Players != null && Players.Count(f => f.IsReady) == 0)
                    _isPlaying = false;
                return _isPlaying;
            }
            set => _isPlaying = value;
        }
        public string MyName { get; set; }
        
        public DieResult LastDiceResult { get; }
                
        public int Turn { get; }
        
        public string Password { get; set; }

        public List<Player> Players { get; }
        public int PlayersNumber { get; }

        public bool RerollMode { get; set; }
        
        public Rule Rules { get; set; }
        #endregion

        #region Methods

        public void ApplyScore(IRollResult result)
        {
            throw new NotImplementedException();
        }

        public void ChangeStyle(IPlayer player, DiceStyle style)
        {
            throw new NotImplementedException();
        }

        public void DoTurn()
        {
            throw new NotImplementedException();
        }

        public void FixAllDices(int value, bool isfixed)
        {
            throw new NotImplementedException();
        }

        public void FixDice(int value, bool isfixed)
        {
            throw new NotImplementedException();
        }

        public bool IsDiceFixed(int value)
        {
            throw new NotImplementedException();
        }
        
        public void JoinGame(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void ManualChange(bool isfixed, int oldvalue, int newvalue)
        {
            throw new NotImplementedException();
        }

        public void NextTurn()
        {
            throw new NotImplementedException();
        }
        
        public void ReportMagictRoll()
        {
            throw new NotImplementedException();
        }

        public void ReportRoll()
        {
            throw new NotImplementedException();
        }
        
        public void ResetRolls()
        {
            throw new NotImplementedException();
        }

        public void RestartGame()
        {
            throw new NotImplementedException();
        }
        
        public void LeaveGame(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerReady(IPlayer player, bool isready)
        {
            throw new NotImplementedException();
        }

        public void SetPlayerReady(bool isready)
        {
            throw new NotImplementedException();
        }

        public void SendChatMessage(ChatMessage message)
        {
            throw new NotImplementedException();
        }
        #endregion        
    }
}