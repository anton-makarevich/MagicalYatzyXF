using System;
using System.Linq;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game.DieResultExtensions;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class YatzyGame: IGame
    {
        private bool _isPlaying;
        private int[] _lastRollResults;
        private List<int> fixedRollResults = new List<int>();
        private Queue<int> thisTurnValues = new Queue<int>();
        private bool _reRollMode;

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
        
        public int NumberOfFixedDice => fixedRollResults?.Count ?? 0;

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

        public int Roll
        {
            get
            {
                var roll = 1;
                if (Players == null) return roll;
                foreach (var p in Players)
                {
                    if (p.Roll > roll)
                        roll = p.Roll;
                }
                return roll;
            }
        }
        
        public string MyName { get; set; }
        
        public DieResult LastDiceResult => new DieResult() 
        {
            DiceResults = _lastRollResults?.ToList() ?? new List<int>()
        };
                
        public int Turn { get; private set; }
        
        public string Password { get; set; }

        public List<Player> Players { get; }
        public int NumberOfPlayers => Players?.Count ?? 0;

        public bool ReRollMode
        {
            get => _reRollMode;
            set
            {
                _reRollMode = value;
                if (!value)
                    thisTurnValues = new Queue<int>();
                else
                    fixedRollResults = new List<int>();
            }
        }
        
        public Rule Rules { get; }
        #endregion

        #region Methods

        public void ApplyScore(IRollResult result)
        {
            //check for kniffel bonus
            if (Rules.HasExtendedBonuses && result.ScoreType != Scores.Kniffel)
            {
                //check if already have kniffel
                var kresult = CurrentPlayer.GetResultForScore(Scores.Kniffel);
                result.HasBonus = (LastDiceResult.YatzyFiveOfAKindScore() == 50 && kresult.Value==kresult.MaxValue);
            }/*
            //sending result to everyone
            if (ResultApplied != null)
                ResultApplied(this, new ResultEventArgs(CurrentPlayer, result));
            //update players results on server
#if SERVER
            result.Value = result.PossibleValue;
            var cr =CurrentPlayer.Results.FirstOrDefault(f => f.ScoreType == result.ScoreType);
            cr=  result;
            _roundTimer.Stop();
#endif
            //check for numeric bonus and apply it
            if (Rules.HasStandardBonus)
            {
                var bonusResult=CurrentPlayer.Results.FirstOrDefault(f=>f.ScoreType== KniffelScores.Bonus);
                if (result.IsNumeric && !bonusResult.HasValue)
                {
                    if (CurrentPlayer.TotalNumeric > 62)
                    {
                        bonusResult.PossibleValue = 35;
                        ResultApplied(this, new ResultEventArgs(CurrentPlayer, new RollResult()
                        {
                            ScoreType = KniffelScores.Bonus,
                            PossibleValue = bonusResult.PossibleValue
                        }));
#if SERVER
                        bonusResult.Value = bonusResult.PossibleValue;
#endif
                    }
                    else if (CurrentPlayer.TotalNumeric + CurrentPlayer.MaxRemainingNumeric < 63)
                    {
                        bonusResult.PossibleValue = 0;
                        ResultApplied(this, new ResultEventArgs(CurrentPlayer, new RollResult()
                        {
                            ScoreType = KniffelScores.Bonus,
                            PossibleValue = bonusResult.PossibleValue
                        }));
#if SERVER
                        bonusResult.Value = bonusResult.PossibleValue;
#endif
                    }
                    
                }
            }

            DoMove();*/
           
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