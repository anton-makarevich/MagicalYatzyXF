using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using static Sanet.MagicalYatzy.Models.Events.GameEvents;

namespace Sanet.MagicalYatzy.Models.Game
{
    /// <summary>
    /// Dice Panel Control - where we are rolling dice
    /// </summary>
    public class DicePanel : IDicePanel
    {
        private const int MaxAttemptsToFindDicePosition = 1000;

        #region Fields
        private int _diceCount = 6;
        private Die _lastClickedDie;

        private readonly IGameSettingsService _gameSettingsService;
        #endregion

        #region Events

        public event EventHandler<DiceFixedEventArgs> DieFixed;
        public event DieChangedEventHandler DieChangedManually;
        public event DieManualChangeRequestEventHandler DieManualChangeRequested;

        public event EventHandler RollEnded;
        public event EventHandler RollStarted;
        public event EventHandler<Die> DieAdded;
        public event EventHandler<Die> DieRemoved;

        internal bool AreDiceGenerated => (Dice != null);

        #endregion

        #region Properties
        
        public bool PlaySound { get; set; }
        public DiceStyle PanelStyle { get; } = DiceStyle.Classic;
        public List<Die> Dice { get; set; } = new();
        /// <summary>
        /// Number of Dice in the Panel
        /// </summary>
        public int DiceCount
        {
            get => _diceCount;
            set
            {
                _diceCount = value;

                GenerateDice();
            }
        }

        /// <summary>
        /// Draws a Box Around the Die for Collision Debugging
        /// </summary>
        public bool DebugDrawMode { get; set; } = false;

        /// <summary>
        /// Allows user to click dice to lock their movement
        /// </summary>
        public bool ClickToFix { get; set; } 

        /// <summary>
        /// Summed Result of All the Dice
        /// </summary>
        public DieResult Result => new DieResult { DiceResults = Dice.Select(d=>d.Result).ToList() };

        public bool AreAllDiceStopped => !Dice.Any(d => d.IsRolling || d.IsLanding);

        public bool IsRolling => Dice.Any(d => d.IsRolling);

        public bool WithSound { get; set; } = false;
        
        public bool ManualSetMode { get; set; } 

        public Rectangle Bounds { get; private set; }

        public int FixedDiceCount => Dice.Count(d => d.IsFixed);

        public bool AreAllDiceFixed => FixedDiceCount == Dice.Count;
        public Thickness SaveMargins { get; set; } = new();

        #endregion

        #region Constructor
        public DicePanel(IGameSettingsService gameSettingsService)
        {
            _gameSettingsService = gameSettingsService;
            GenerateDice();
        }
        #endregion

        #region Methods
        public void ChangeDiceManually(int newValue)
        {
            var oldValue = _lastClickedDie.Result;
            _lastClickedDie.Result = newValue;
            _lastClickedDie.DrawDie();
            ManualSetMode = false;
            DieChangedManually?.Invoke(_lastClickedDie.IsFixed, oldValue, _lastClickedDie.Result);
        }

        public void ChangeDice(int oldValue, int newValue)
        {
            var diceToChange = Dice.FirstOrDefault(f => f.Result == oldValue);
            if (diceToChange == null)
                return;
            diceToChange.Result = newValue;
            diceToChange.DrawDie();
        }

        /// <summary>
        /// Try to clear everything
        /// </summary>
        public void Dispose()
        {
            DiceCount = 0;
            Dice.Clear();
            Dice = null;
        }

        public bool RollDice(List<int> overrideValues)
        {
            if (ManualSetMode)
                ManualSetMode = false;

            //don't roll if all are fixed
            if (AreAllDiceFixed)
            {
                return false;
            }

            RollStarted?.Invoke(this,null);

            var shouldOverrideValues = overrideValues != null && 
                overrideValues.Count == Dice.Count;

            //first values for fixed dices
            var fixedDiceCount = Dice.Count(f => f.IsFixed);

            for (var i = 0; i <= Dice.Count - 1; i++)
            {
                var initialResultValue = 0;
                var d = Dice[i];
                if (d.IsFixed)
                    continue;
                if (shouldOverrideValues)
                {
                    initialResultValue = overrideValues[fixedDiceCount];
                    fixedDiceCount += 1;
                }

                d.InitializeRoll(initialResultValue);
            }

            // Start playing the animation loop
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            BeginLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }

        public void DieClicked(Point clickLocation)
        {
            //determine if die was clicked
            _lastClickedDie = null;
            foreach (var dice in Dice)
            {
                if (!dice.ClickedOn(clickLocation)) continue;
                _lastClickedDie = dice;
                break;
            }
            //no die was clicked
            if (_lastClickedDie == null)
                return;

            if (ManualSetMode)
            {
                DieManualChangeRequested?.Invoke(ChangeDiceManually);
            }
            else if (ClickToFix)
            {
                _lastClickedDie.IsFixed = !_lastClickedDie.IsFixed;
                _lastClickedDie.DrawDie();
                DieFixed?.Invoke(this, new DiceFixedEventArgs(_lastClickedDie.IsFixed, _lastClickedDie.Result));
            }
        }

        public void FixDice(int index, bool isFixed)
        {
            foreach (var dice in Dice)
            {
                if (dice.Result != index || dice.IsFixed != !isFixed) continue;
                dice.IsFixed = isFixed;
                dice.DrawDie();
                return;
            }
        }

        public void UnfixAll()
        {
            foreach (var dice in Dice)
            {
                if (!dice.IsFixed) continue;
                dice.IsFixed = false;
                dice.DrawDie();
            }
        }

        public void Resize(int width, int height)
        {
            if (Math.Abs(width - Bounds.Width) < 0.1 && Math.Abs(height - Bounds.Height) < 0.1)
                return;

            Bounds = new Rectangle(0, 0, width, height);

            foreach (var dice in Dice)
            {
                if (!Bounds.Contains(dice.Bounds) || dice.Bounds.Position.IsZero )
                    FindDiePosition(dice);
            }
        }

        public Point? GetDicePosition(int diceValue)
        {
            var dice = Dice.FirstOrDefault(f => f.Result == diceValue);
            if (dice == null)
                return null;
            return new Point(dice.PosX,dice.PosY);
        }
        #endregion

        #region Private methods

        private void GenerateDice()
        {
            if (Dice != null && Dice.Any())
            {
                foreach (var die in Dice)
                {
                    DieRemoved?.Invoke(this,die);
                }
            }

            Dice = new List<Die>();

            while (Dice.Count < DiceCount)
            {
                var dice = new Die(this, _gameSettingsService, new RandomValueGenerator());

                FindDiePosition(dice);

                Dice.Add(dice);

                DieAdded?.Invoke(this, dice);
            }
        }

        private void FindDiePosition(Die die)
        {
            bool isDone;
            var attempt = 0;

            if (Bounds.Height < die.Bounds.Height || Bounds.Width < die.Bounds.Width)
                return;

            do
            {
                attempt += 1;
                isDone = true;
                die.InitializePosition();
                foreach (var otherDie in Dice)
                {
                    isDone = !die.Overlapping(otherDie);
                }
            } while (!(isDone | attempt > MaxAttemptsToFindDicePosition));
            die.DrawDie();
        }

        private async Task BeginLoop()
        {
            await Task.Delay(_gameSettingsService.DieSpeed);
            OnLoopCompleted();
        }

        private void OnLoopCompleted()
        {
            foreach (var dice in Dice)
            {
                dice.UpdateDiePosition();
                dice.DrawDie();
            }

            HandleCollisions();

            if (!AreAllDiceStopped)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                BeginLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                RollEnded?.Invoke(this,null);
            }
        }

        private void HandleCollisions()
        {
            int i;
            
            //can't use foreach loops here, want to start j loop index AFTER first loop
            for (i = 0; i <= Dice.Count - 2; i++)
            {
                int j;
                for (j = i + 1; j <= Dice.Count - 1; j++)
                {
                    var di = Dice[i];
                    var dj = Dice[j];
                    di.HandleCollision(dj);
                }
            }
        }

        #endregion
    }
}
