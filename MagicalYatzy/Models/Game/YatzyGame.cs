using System;
using System.Linq;
using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MagicalYatzy.Models.Game.Magical;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class YatzyGame: IGame
    {
        //sync object
        private readonly object _syncRoot = new object();
        
        private readonly IDiceGenerator _diceGenerator = new RandomDiceGenerator();
        
        private bool _isPlaying;
        private int[] _lastRollResults;
        private List<int> _fixedRollResults = new List<int>();
        private Queue<int> _thisTurnValues = new Queue<int>();
        private bool _reRollMode;
        private readonly Random _randomizer = new Random();

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
        public event EventHandler<ChatMessageEventArgs> ChatMessageSent;
        
        public event EventHandler<PlayerEventArgs> PlayerJoined;
        
        public event EventHandler<ResultEventArgs> ResultApplied;
        #endregion

        #region Properties

        public IPlayer CurrentPlayer { get; private set; } 
        
        public string GameId { get; }
        
        public int NumberOfFixedDice => _fixedRollResults?.Count ?? 0;

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
        
        public DieResult LastDiceResult => new DieResult() 
        {
            DiceResults = _lastRollResults?.ToList() ?? new List<int>()
        };
                
        public int Round { get; private set; }

        public List<Player> Players { get; private set; }
        public int NumberOfPlayers => Players?.Count ?? 0;

        public bool ReRollMode //TODO can setter be private?
        {
            get => _reRollMode;
            set
            {
                _reRollMode = value;
                if (!value)
                    _thisTurnValues = new Queue<int>();
                else
                    _fixedRollResults = new List<int>();
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
                    var possibleBonusValue = -1;
                    if (CurrentPlayer.TotalNumeric > 62)
                        possibleBonusValue = 35;
                    else if (CurrentPlayer.TotalNumeric + CurrentPlayer.MaxRemainingNumeric < 63)
                        possibleBonusValue = 0;
       
                    if (possibleBonusValue > -1)
                    {
                        bonusResult.PossibleValue = possibleBonusValue;
                        ResultApplied?.Invoke(this, new ResultEventArgs(CurrentPlayer, bonusResult));
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
            if (player==null)
                return;
            player = Players.FirstOrDefault(f => f.InGameId == player.InGameId);
            if (player == null) return;
            player.SelectedStyle = style;
            StyleChanged?.Invoke(null, new PlayerEventArgs(player));
        }

        public void DoTurn()
        {
            _fixedRollResults = new List<int>();
            
            if (Rules.CurrentRule == Game.Rules.krMagic)
                ReRollMode = false;
            //if we have current player - round is continuing, so selecting next
            var currentSeatNo = 0;
            if (CurrentPlayer != null)
            {
                CurrentPlayer.IsMyTurn = false;
                //if player left we can't just take next - need to check to the last possible place
                currentSeatNo = CurrentPlayer.SeatNo + 1;
            }
            for (var seatNo = currentSeatNo; seatNo < 5; seatNo++)
            {
                CurrentPlayer = Players.Where(f => f.IsReady).FirstOrDefault(f => f.SeatNo == seatNo);
                if (CurrentPlayer != null)
                    break;
            }
            //if current player is null then all players are done in this round - start next
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

        public void FixAllDice(int value, bool isFixed)
        {
            lock (_syncRoot)
            {
                if (isFixed)
                {
                    var numberOfDiceToFix = LastDiceResult.NumDiceOf(value);
                    var fixedCount = _fixedRollResults.Count(f => f == value);
                    for (var counter = 0; counter < (numberOfDiceToFix - fixedCount); counter++)
                    {
                        _fixedRollResults.Add(value);
                        DiceFixed?.Invoke(this, new FixDiceEventArgs(CurrentPlayer, value, true));
                    }
                }
                else
                {
                    while (_fixedRollResults.Contains(value))
                    {
                        _fixedRollResults.Remove(value);
                        DiceFixed?.Invoke(this, new FixDiceEventArgs(CurrentPlayer, value, false));
                    }
                }
            }
        }

        public void FixDice(int value, bool isFixed)
        {
            lock (_syncRoot)
            {
                if (isFixed)
                {
                    var numberOfDiceOfValue = LastDiceResult.NumDiceOf(value);
                    var fixedCount = _fixedRollResults.Count(f => f == value);
                    if (numberOfDiceOfValue > fixedCount)
                        _fixedRollResults.Add(value);
                }
                else
                {
                    if (_fixedRollResults.Contains(value))
                        _fixedRollResults.Remove(value);
                }

                DiceFixed?.Invoke(this, new FixDiceEventArgs(CurrentPlayer, value, isFixed));
            }
        }

        public bool IsDiceFixed(int value)
        {
            return _fixedRollResults.Contains(value);
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
                player.PrepareForGameStart(Rules);
                Players.Add(player as Player);
                PlayerJoined?.Invoke(this, new PlayerEventArgs(player));
            }
        }

        public void ManualChange(int oldValue, int newValue, bool isFixed)
        {
            if (Rules.CurrentRule != Game.Rules.krMagic)
                return;

            lock (_syncRoot)
            {
                var oldIndexValue = _lastRollResults.ToList().IndexOf(oldValue);

                if (oldIndexValue <= -1) return;
                _lastRollResults[oldIndexValue] = newValue;
                if (isFixed && _fixedRollResults.Contains(oldValue))
                {
                    FixDice(oldValue, false);
                    FixDice(newValue, true);
                }

                DiceChanged?.Invoke(this, new RollEventArgs(CurrentPlayer, _lastRollResults));
            }
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
        
        public void ReportMagicRoll()
        {
            if (Rules.CurrentRule != Game.Rules.krMagic)
                return;
            var artifact = CurrentPlayer.AvailableMagicalArtifacts?.FirstOrDefault(f => f.Type == Artifacts.MagicalRoll);
            if (artifact == null)
                return;
            lock (_syncRoot)
            {
                //set magic value
                //1) check how many free hands we have
                var freeHands = Rule.PokerHands.Where(score => !CurrentPlayer.GetResultForScore(score).HasValue).ToList();
                if (freeHands.Count==0)
                    ReportRoll();// no available hands - just roll
                else
                {
                    _lastRollResults = freeHands[_randomizer.Next(freeHands.Count)].GetMagicResults();

                    MagicRollUsed?.Invoke(this, new PlayerEventArgs(CurrentPlayer));
                    //CurrentPlayer.OnMagicRollUsed();
                    foreach (var result in _lastRollResults)
                        _thisTurnValues.Enqueue(result);
                    //roll report
                    DiceRolled?.Invoke(this, new RollEventArgs(CurrentPlayer, _lastRollResults));
                }
            }
        }

        public void ReportRoll()
        {
            lock (_syncRoot)
            {
                var diceIndexToSet = 0;
                _lastRollResults = new int[5];
                for (var diceCounter = diceIndexToSet; diceCounter < _fixedRollResults.Count; diceCounter++)
                {
                    _lastRollResults[diceCounter] = _fixedRollResults[diceCounter];
                }
                diceIndexToSet = _fixedRollResults.Count;

                for (var diceCounter = diceIndexToSet; diceCounter <= 4; diceCounter++)
                {
                    var diceValue = _diceGenerator.GetNextDiceResult(_fixedRollResults.ToArray());

                    _lastRollResults[diceCounter] = diceValue;
                    if (Rules.CurrentRule != Game.Rules.krMagic) continue;
                    if (!ReRollMode)
                        _thisTurnValues.Enqueue(diceValue);
                    else if (_thisTurnValues.Count>0)
                        _lastRollResults[diceCounter]=_thisTurnValues.Dequeue();
                }

                DiceRolled?.Invoke(this, new RollEventArgs(CurrentPlayer, _lastRollResults));
            }
        }
        
        public void ResetRolls()
        {
            CurrentPlayer.Roll = 1;
            ReRollMode = true;
            PlayerRerolled?.Invoke(null, new PlayerEventArgs(CurrentPlayer));
        }

        public void RestartGame()
        {
            lock (_syncRoot)
            {
                IsPlaying = false;
                Round = 1;

                for (var playerIndex = 0;playerIndex<NumberOfPlayers;playerIndex++)
                {
                    var player = Players[playerIndex];
                    player.Roll = 1;
                    player.SeatNo=playerIndex-1;
                    if (player.SeatNo < 0 )
                        player.SeatNo = NumberOfPlayers - 1;
                    player.PrepareForGameStart(Rules);
#if !SERVER
                    player.IsReady = true;
#endif
                }
                Players = Players.OrderBy(f => f.SeatNo).ToList();
                CurrentPlayer = null;
                
                StartGame();
            }
        }
        
        public void LeaveGame(IPlayer player)
        {
            player = Players.FirstOrDefault(f => f.InGameId == player.InGameId);
            if (player == null)
                return;

            Players.Remove((Player) player);

            PlayerLeft?.Invoke(null, new PlayerEventArgs(player));
            if (CurrentPlayer != null && CurrentPlayer.InGameId == player.InGameId && IsPlaying)
            {
#if SERVER
                _roundTimer.Stop();
#endif
                DoTurn();
                return;
            }
            StartGame();
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
            if (!isReady) return;
            
            StartGame();
        }

        public void SendChatMessage(ChatMessage message)
        {
            ChatMessageSent?.Invoke(this, new ChatMessageEventArgs(message));
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