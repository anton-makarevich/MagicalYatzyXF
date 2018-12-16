using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Sanet.MagicalYatzy.Models.Events.GameEvents;

namespace Sanet.MagicalYatzy.Models.Game
{
    /// <summary>
    /// Dice Panel Control - where we are rolling dices
    /// </summary>
    public class DicePanel : IDicePanel
    {
        private const int MaxAttemptsToFindDicePosition = 1000;

        #region Fields
        private int _diceCount = 6;
        internal Die _lastClickedDie;

        private readonly IGameSettingsService _gameSettingsService;
        #endregion

        #region Events

        public event DieFrozenEventHandler DieFixed;
        public event DieChangedEventHandler DieChangedManually;
        public event DieManualChangeRequestEventHandler DieManualChangeRequested;

        public event Action RollEnded;
        public event Action RollStarted;
        public event Action<Die> DieAdded;
        public event Action<Die> DieRemoved;

        internal bool AreDiceGenerated => (Dice != null);

        #endregion

        #region Properties
        public static double DeviceScale = 1;
        public bool TreeDScale { get; set; }
        public double TreeDScaleCoef { get; set; }
        public bool PlaySound { get; set; }
        public DiceStyle PanelStyle { get; } = DiceStyle.Classic;
        public List<Die> Dice { get; set; } = new List<Die>();
        /// <summary>
        /// Number of Dice in the Panel
        /// </summary>
        public int DiceCount
        {
            get { return _diceCount; }
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
        public bool ClickToFix { get; set; } = false;

        /// <summary>
        /// Summed Result of All the Dice
        /// </summary>
        public DieResult Result => new DieResult { DiceResults = Dice.Select(d=>d.Result).ToList() };

        public bool AreAllDiceStopped => !Dice.Any(d => d.IsRolling || d.IsLanding);

        internal bool IsRolling => Dice.Any(d => d.IsRolling);

        public bool WithSound { get; set; } = false;

        public int RollDelay { get; set; } = 20;

        public bool ManualSetMode { get; set; } = false;

        public Rectangle Bounds { get; private set; }

        public int FixedDiceCount => Dice.Count(d => d.IsFixed);

        public bool AreAllDiceFixed => FixedDiceCount == Dice.Count;

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
            int oldvalue = _lastClickedDie.Result;
            _lastClickedDie.Result = newValue;
            _lastClickedDie.DrawDie();
            ManualSetMode = false;
            DieChangedManually?.Invoke(_lastClickedDie.IsFixed, oldvalue, _lastClickedDie.Result);
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

            RollStarted?.Invoke();

            var shouldOverrideValues = overrideValues != null && 
                overrideValues.Count == Dice.Count;

            //first values for fixed dices
            int fixedDiceCount = Dice.Count(f => f.IsFixed);

            for (int i = 0; i <= Dice.Count - 1; i++)
            {
                int initialResulrValue = 0;
                var d = Dice[i];
                if (d.IsFixed)
                    continue;
                if (shouldOverrideValues)
                {
                    initialResulrValue = overrideValues[fixedDiceCount];
                    fixedDiceCount += 1;
                }

                d.InitializeRoll(initialResulrValue);
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
            foreach (Die d in Dice)
            {
                if (d.ClickedOn(clickLocation))
                {
                    _lastClickedDie = d;
                    break;
                }
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
                DieFixed?.Invoke(_lastClickedDie.IsFixed, _lastClickedDie.Result);
            }
        }

        public void FixDice(int index, bool isfixed)
        {
            foreach (Die d in Dice)
            {
                if (d.Result == index && d.IsFixed == !isfixed)
                {
                    d.IsFixed = isfixed;
                    d.DrawDie();
                    return;
                }
            }
        }

        public void UnfixAll()
        {
            foreach (Die dice in Dice)
            {
                if (dice.IsFixed)
                {
                    dice.IsFixed = false;
                    dice.DrawDie();
                }
            }
        }

        public void Resize(int width, int height)
        {
            if (Math.Abs(width - Bounds.Width) < 0.1 && Math.Abs(height - Bounds.Height) < 0.1)
                return;

            Bounds = new Rectangle(0, 0, width, height);

            foreach (Die d in Dice)
            {
                if (!Bounds.Contains(d.Bounds) || d.Bounds.Position.IsZero )
                    FindDiePosition(d);
            }
        }
        #endregion

        #region Private methods

        private void GenerateDice()
        {
            if (Dice != null && Dice.Any())
            {
                foreach (var die in Dice)
                {
                    DieRemoved?.Invoke(die);
                }
            }

            Dice = new List<Die>();

            while (Dice.Count < DiceCount)
            {
                var d = new Die(this, _gameSettingsService);

                FindDiePosition(d);

                Dice.Add(d);

                DieAdded?.Invoke(d);
            }
        }

        private void FindDiePosition(Die die)
        {
            bool _isDone = false;
            int attempt = 0;

            if (Bounds.Height < die.Bounds.Height || Bounds.Width < die.Bounds.Width)
                return;

            do
            {
                attempt += 1;
                _isDone = true;
                die.InitializeLocation();
                foreach (Die otherDie in Dice)
                {
                    _isDone = !die.Overlapping(otherDie);
                }
            } while (!(_isDone | attempt > MaxAttemptsToFindDicePosition));
            die.DrawDie();
        }

        private async Task BeginLoop()
        {
            await Task.Delay(RollDelay);
            OnLoopCompleted();
        }

        private void OnLoopCompleted()
        {
            foreach (Die d in Dice)
            {
                d.UpdateDiePosition();
                d.DrawDie();
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
                RollEnded?.Invoke();
            }
        }

        private void HandleCollisions()
        {
            Die di = null;
            Die dj = null;
            int i = 0;
            int j = 0;

            if (DiceCount == 1)
                return;

            //can't use foreach loops here, want to start j loop index AFTER first loop
            for (i = 0; i <= Dice.Count - 2; i++)
            {
                for (j = i + 1; j <= Dice.Count - 1; j++)
                {
                    if (i == j)
                        continue;
                    di = Dice[i];
                    dj = Dice[j];
                    di.HandleCollision(dj);
                }
            }
        }

        #endregion
    }
}
