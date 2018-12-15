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

        #region Fields
        public List<Die> Dice = new List<Die>();

        
        private bool _withSound = false;
        
        private int _diceCount = 0;
        Die _lastClickedDie;
        private bool _ManualSetMode = false;

        private readonly IGameSettingsService _gameSettingsService;
        #endregion

        #region Events

        public event DieFrozenEventHandler DieFrozen;
        public event DieChangedEventHandler DieChangedManually;
        public event Action RollEnded;
        public event Action RollStarted;
        public event Action<Die> DieAdded;
        public event Action<Die> DieRemoved;

        #endregion

        #region Properties
        public static double DeviceScale = 1;
        public bool TreeDScale { get; set; }
        public double TreeDScaleCoef { get; set; }
        public bool PlaySound { get; set; }
        public DiceStyle PanelStyle { get; } = DiceStyle.Classic;

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
        public bool ClickToFreeze { get; set; } = false;

        /// <summary>
        /// Summed Result of All the Dice
        /// </summary>
        public DieResult Result
        {
            get
            {
                var dr = new List<int>();

                foreach (Die d in Dice)
                {
                    dr.Add(d.Result);

                }
                return new DieResult { DiceResults = dr };
            }
        }

        public bool AllDiceStopped
        {
            get
            {
                foreach (Die d in Dice)
                {
                    if (d.IsRolling || d.IsLanding)
                        return false;
                }

                return true;
            }
        }

        private bool IsRolling
        {
            get
            {
                foreach (Die d in Dice)
                    if (d.IsRolling) return true;
                return false;
            }
        }

        public bool WithSound
        {
            get { return _withSound; }
            set
            {
                if (_withSound != value)
                {
                    _withSound = value;

                }
            }
        }

        public int RollDelay { get; set; } = 20;

        public bool ManualSetMode
        {
            get { return _ManualSetMode; }
            set
            {
                if (_ManualSetMode != value)
                {
                    _ManualSetMode = value;
                }
            }
        }
        
        public Rectangle Bounds { get; private set; }
        
        #endregion

        #region Constructor
        public DicePanel(IGameSettingsService gameSettingsService)
        {
            _gameSettingsService = gameSettingsService;
        }
        #endregion

        #region Methods
        void _popup_Closed(object sender, object e)
        {
            int oldvalue = _lastClickedDie.Result;

            _lastClickedDie.DrawDie();
            ManualSetMode = false;
            DieChangedManually?.Invoke(_lastClickedDie.IsFrozen, oldvalue, _lastClickedDie.Result);
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
            Dice.Clear();
        }

        private void GenerateDice()
        {
            if (Dice!=null && Dice.Any())
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
                    if (die.Overlapping(otherDie))
                    {
                        _isDone = false;
                    }
                }
            } while (!(_isDone | attempt > 1000));
            die.DrawDie();
        }

        public bool RollDice(List<int> aResults)
        {
            if (ManualSetMode)
                ManualSetMode = false;

            //don't roll if all frozen
            if (AllDiceFrozen())
            {
                //LogManager.Log(LogLevel.Message, "DicePanel.RollDice","Can't roll... allfixed");
                return false;
            }
            RollStarted?.Invoke();
            //first values for fixed dices
            int j = Dice.Count(f => f.IsFrozen);

            for (int i = 0; i <= Dice.Count - 1; i++)
            {
                int iResult = 0;
                var d = Dice[i];
                if (d.IsFrozen)
                    continue;
                if ((aResults != null))
                {
                    if (aResults.Count == Dice.Count)
                    {
                        iResult = aResults[j];
                        j += 1;
                    }

                }
                //d.iSound = FRand.Next(1, 10);
                d.InitializeRoll(iResult);
            }

            // Start playing the Storyboard loop

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            BeginLoop();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
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

            if (!AllDiceStopped)
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
        
        public void DieClicked(object sender, IEnumerable<Point> e)
        {
            Point pointClicked = e.First();
            //determine if die was clicked
            _lastClickedDie = null;
            foreach (Die d in Dice)
            {
                if (d.ClickedOn(pointClicked.X, pointClicked.Y))
                {
                    _lastClickedDie = d;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            //no die was clicked
            if (_lastClickedDie == null)
                return;

            if (ManualSetMode)
            {
                /*_selectionPanel.SelectedDice = _lastClickedDie;
                _selectionPanel.Draw();
                _popup.IsOpen = true;*/
            }
            else if (ClickToFreeze)
            {
                _lastClickedDie.IsFrozen = !_lastClickedDie.IsFrozen;
                _lastClickedDie.DrawDie();
                if (DieFrozen != null)
                {
                    DieFrozen(_lastClickedDie.IsFrozen, _lastClickedDie.Result);
                }
            }
        }

        private bool DiceGenerated()
        {
            return (Dice != null);
        }

        public int FrozenCount()
        {
            int num = 0;
            foreach (Die d in Dice)
                if (d.IsFrozen) num++;
            return num;
        }

        //don't roll if all frozen
        public bool AllDiceFrozen()
        {
            foreach (Die d in Dice)
            {
                if (!d.IsFrozen)
                {
                    return false;
                }
            }
            return true;
        }
        public void FixDice(int index, bool isfixed)
        {
            foreach (Die d in Dice)
            {
                if (d.Result == index && d.IsFrozen == !isfixed)
                {
                    d.IsFrozen = isfixed;
                    d.DrawDie();
                    return;
                }
            }
        }

        public void ClearFreeze()
        {
            Die d = null;

            foreach (Die d_loopVariable in Dice)
            {
                d = d_loopVariable;
                if (d.IsFrozen)
                {
                    d.IsFrozen = false;
                    d.DrawDie();
                }
            }

        }

        public void Resize(int width, int height)
        {
            if (width == Bounds.Width && height == Bounds.Height)
                return;

            Bounds = new Rectangle(0, 0, width, height);

            foreach (Die d in Dice)
            {
                if (!Bounds.Contains(d.Bounds) || d.Bounds.Position.IsZero )
                    FindDiePosition(d);
            }
        }
        #endregion
    }


}
