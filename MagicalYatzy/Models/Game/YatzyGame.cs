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
        //sync object
        readonly object _syncRoot = new object();
        
        private bool _isPlaying;
        private int[] _lastRollResults;
        private List<int> fixedRollResults = new List<int>();
        private Queue<int> thisTurnValues = new Queue<int>();
        private bool _reRollMode;

        public YatzyGame(Rules rules)
        {
            Rules = new Rule(rules);
            Players = new List<Player>();
            GameId = Guid.NewGuid().ToString("N");
        }

        public YatzyGame():this(Game.Rules.krExtended)
        {
        }
        
        #region Events
        public event EventHandler GameUpdated;
        public event EventHandler<PlayerEventArgs> PlayerLeft;
        public event EventHandler<RollEventArgs> DiceChanged;
        public event EventHandler<FixDiceEventArgs> DiceFixed;
        public event EventHandler<RollEventArgs> DiceRolled;
        public event EventHandler<PlayerEventArgs> PlayerReady;
        public event EventHandler<PlayerEventArgs> PlayerRerolled;
        public event EventHandler<PlayerEventArgs> MagicRollUsed;
        public event EventHandler<PlayerEventArgs> StyleChanged;
        
        public event EventHandler GameFinished;
        
        public event EventHandler<MoveEventArgs> TurnChanged;
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
                
        public int Round { get; private set; }
        
        public string Password { get; set; }

        public List<Player> Players { get; private set; }
        public int NumberOfPlayers => Players?.Count ?? 0;

        public bool ReRollMode //TODO can setter be private?
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
                var kniffelResult = CurrentPlayer.GetResultForScore(Scores.Kniffel);
                result.HasBonus = (LastDiceResult.YatzyFiveOfAKindScore() == 50 && kniffelResult.Value==kniffelResult.MaxValue);
            }
            //sending result to everyone
            ResultApplied?.Invoke(this, new ResultEventArgs(CurrentPlayer, result));
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
                var bonusResult=CurrentPlayer.Results?.FirstOrDefault(f=>f.ScoreType== Scores.Bonus);
                if (bonusResult != null && (result.IsNumeric && !bonusResult.HasValue))
                {
                    if (CurrentPlayer.TotalNumeric > 62)
                    {
                        bonusResult.PossibleValue = 35;
                        ResultApplied?.Invoke(this, new ResultEventArgs(CurrentPlayer, new RollResult(Scores.Bonus)
                        {
                            PossibleValue = bonusResult.PossibleValue
                        }));
#if SERVER
                        bonusResult.Value = bonusResult.PossibleValue;
#endif
                    }
                    else if (CurrentPlayer.TotalNumeric + CurrentPlayer.MaxRemainingNumeric < 63)
                    {
                        bonusResult.PossibleValue = 0;
                        ResultApplied?.Invoke(this, new ResultEventArgs(CurrentPlayer, new RollResult(Scores.Bonus)
                        {
                            PossibleValue = bonusResult.PossibleValue
                        }));
#if SERVER
                        bonusResult.Value = bonusResult.PossibleValue;
#endif
                    }
                    
                }
            }

            DoTurn();
        }

        public void ChangeStyle(IPlayer player, DiceStyle style)
        {
            throw new NotImplementedException();
        }

        public void DoTurn()
        {
            fixedRollResults = new List<int>();
            
            if (Rules.CurrentRule == Game.Rules.krMagic)
                ReRollMode = false;
            //if we have current player - round is continuing, so selecting next
            if (CurrentPlayer != null)
            {
                CurrentPlayer.IsMyTurn = false;
                //if player left we can't just take next - need to check to the last possible place
                for (var i = CurrentPlayer.SeatNo + 1; i < 5; i++)
                {
                    CurrentPlayer = Players.Where(f => f.IsReady).FirstOrDefault(f => f.SeatNo == i);
                    if (CurrentPlayer != null)
                        break;
                }
            }
            else//else it's new move and we select first player as current
                CurrentPlayer = Players.FirstOrDefault(f => f.SeatNo == 0); //TODO refactor this as first player also can leave
            //if current player null then all players are done in this move - move next
            if (CurrentPlayer == null)
            {
                NextTurn();
                return;
            }
            CurrentPlayer.IsMyTurn = true;

#if SERVER
            _roundTimer.Stop();
            _roundTimer.Start();
#endif
            //report to all that player changed
            TurnChanged?.Invoke(this, new MoveEventArgs(CurrentPlayer, Round));
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
            lock (_syncRoot)
            {
                if (Players == null)
                    Players = new List<Player>();

                var seat = 0;
                if (Players.Count(f => f.IsReady) == 0)
                {
                    IsPlaying = false;
                    Round = 1;
#if SERVER
                    _roundTimer.Stop();
#endif
                }
                while (Players.FirstOrDefault(f => f.SeatNo == seat) != null)
                    seat++;
               
                player.SeatNo = seat;
                player.InGameId = Guid.NewGuid().ToString("N");
                Players.Add(player as Player);
                PlayerJoined?.Invoke(this, new PlayerEventArgs(player));
            }
        }

        public void ManualChange(bool isfixed, int oldvalue, int newvalue)
        {
            throw new NotImplementedException();
        }

        public void NextTurn()
        {
            //if current round is last
            if (Round == Rules.MaxRound)
            {
                Players=Players.OrderByDescending(f => f.Total).ToList();
                CurrentPlayer = Players.First();
                IsPlaying = false;
                foreach (var p in Players)
                {
                    SetPlayerReady(p,false);
                }

                GameFinished?.Invoke(this, null);
#if SERVER
                    RestartGame();
#endif
            }
            else
            {
                ReorderSeats();
                Round++;
                DoTurn();
            }
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

        public void SetPlayerReady(IPlayer player, bool isReady)
        {
            // allows to join during the first round
            if (IsPlaying && Round > 1)
                isReady = false;
            var previousPlayer=Players.FirstOrDefault(f => f.InGameId == player.InGameId);
            if (previousPlayer == null) return;
            previousPlayer.IsReady = isReady;
            PlayerReady?.Invoke(null, new PlayerEventArgs(previousPlayer));
            if (isReady)
                StartGame();
        }

        public void SendChatMessage(ChatMessage message)
        {
            throw new NotImplementedException();
        }
        #endregion    
        
        private void StartGame()
        {
            lock (_syncRoot)
            {
                if (IsPlaying)
                    return;

                var isEveryoneReady = Players.Count(p => p.IsReady) == Players.Count;

                if (!isEveryoneReady) return;

                ReorderSeats();
                CurrentPlayer = null;
                Round = 1;
                IsPlaying = true;

                GameUpdated?.Invoke(null, null);

                DoTurn();
            }
        }
        
        private void ReorderSeats()
        {
            var seat = 0;
            foreach (var player in Players.OrderBy(f => f.SeatNo))
            {
                player.SeatNo = seat;
                seat++;
            }
        }
    }
}