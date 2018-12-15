using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels.Base;
using System;
using System.Diagnostics;

namespace Sanet.MagicalYatzy.Models.Game
{
    /// <summary>
    /// Dice oject
    /// </summary>
    public class Die: BindableBase
    {
        private static Random ValueGenerator = new Random();

        const int MAXMOVE = 5;

        private readonly IGameSettingsService _gameSettingsService;
        private readonly IDicePanel _dicePanel;

        #region Constructor
        public Die(IDicePanel dicePanel, IGameSettingsService gameSettingsService)
        {
            _dicePanel = dicePanel;
            _gameSettingsService = gameSettingsService;
        }
        #endregion

        #region Fields
        private int _rollLoop;

        //dimensions
        private int _height = 72;
        private int _width = 72;

        //position
        private int _posX;
        private int _posY;

        //direction
        private int _directionX;
        private int _directionY;

        private DieStatus _status = DieStatus.Stopped;

        private string _rotationString;

        private int _frame;
        private int _result = 1;

        private float _opacity;

        private string _imagePath;

        private bool _isFrozen;

        public int _soundCounter;

        #endregion

        #region Properties
        public string StyleString
        {
            get { return $"{_gameSettingsService.DieStyle.ToPathComponent()}"; }
        }

        private int Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;

                if (_frame < 0)
                    _frame += 36;
                if (_frame > 35)
                    _frame -= 36;
            }
        }

        public int Result
        {
            get { return _result; }
            set
            {
                if (value < 1 | value > 6)
                {
                    throw new Exception($"Unexpected value {value}. Should be in the range 1..6");
                }
                else
                {
                    _result = value;
                }
            }
        }

        private int PosX
        {
            get { return _posX; }
            set
            {
                int ks = 0;

                _posX = value;

                if (_posX < 0 + ks)
                {
                    _posX = 0 + ks;
                    BounceX();
                }
                double MW = 0;
                try
                {
                    MW = _dicePanel.Bounds.Width;
                }
                catch { }
                if (_posX > (MW) - _width - ks)
                {
                    _posX = (int)(MW) - _width - ks;
                    BounceX();
                }
            }
        }

        private int PosY
        {
            get { return _posY; }
            set
            {
                _posY = value;

                if (_posY < 0)
                {
                    _posY = 0;
                    BounceY();
                }
                double MH = 0;
                try
                {
                    MH = _dicePanel.Bounds.Height;
                }
                catch { }
                if (_posY > MH - _height)
                {
                    _posY = (int)(MH) - _height;
                    BounceY();
                }
            }
        }

        public bool IsFixed
        {
            get { return _isFrozen; }

            set { _isFrozen = value; }
        }

        internal DieStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (value == DieStatus.Stopped)
                {
                    _directionX = 0;
                    //stop direction
                    _directionY = 0;

                }
            }
        }

        public float Opacity
        { get { return _opacity; }
            private set
            {
                SetProperty(ref _opacity, value);
            }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            private set
            {
                SetProperty(ref _imagePath, value);
            }
        }

        public bool IsNotRolling => Status == DieStatus.Stopped; 

        public bool IsRolling => Status == DieStatus.Rolling;

        public bool IsLanding => Status == DieStatus.Landing;

        public Rectangle Bounds
        {
            get { return new Rectangle(PosX, PosY, _width, _height); }
        }
        #endregion

        #region Methods
        private String GetFramePicPath()
        {
            try
            {
                return StyleString + _rotationString + Frame.ToString() + ".png";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public void InitializeLocation()
        {
            try
            {
                var w1 = (int)_dicePanel.Bounds.Width;
                var h1 = (int)_dicePanel.Bounds.Height;
                if (w1 > 0 && h1 > 0)
                {
                    int mw = w1 - _width;
                    PosX = ValueGenerator.Next(1, mw);
                    mw = h1 - _height;
                    PosY = ValueGenerator.Next(1, mw);
                }
                else
                {
                    PosX = 0;
                    PosY = 0;
                }
                if (PosX < 0) PosX = 0;
                if (PosY < 0) PosY = 0;
            }
            catch
            {
                PosX = 0;
                PosY = 0;
            }
        }

        public void UpdateDiePosition()
        {
            switch (Status)
            {
                case DieStatus.Landing:
                    Frame -= 1;
                    break;
                case DieStatus.Rolling:
                    //+ or - depending on direction
                    Frame += (1 * Math.Sign(_directionY));
                    break;
                case DieStatus.Stopped:
                    return;
            }


            PosX += _directionX;
            PosY += _directionY;

            _rollLoop += 1;

            switch (Status)
            {
                case DieStatus.Rolling:
                    // Stop when max amount of rolls has been reached
                    if (_rollLoop > _gameSettingsService.MaxRollLoop & ValueGenerator.Next(1, 100) < 10)
                    {
                        Status = DieStatus.Landing;
                        _rollLoop = 0;

                        Frame = Result * 6;
                    }

                    break;
                case DieStatus.Landing:

                    if (_rollLoop > (5 - _gameSettingsService.DieAngle))
                    {
                        Status = DieStatus.Stopped;
                    }
                    break;
            }
        }

        public void InitializeRoll(int iResult = 0)
        {
            if (iResult < 0 | iResult > 6)
                iResult = 0;

            //new
            if (!IsFixed)
            {
                do
                {
                    _directionX = ValueGenerator.Next(-MAXMOVE, MAXMOVE + 1);
                } while (!(Math.Abs(_directionX) > 2));
                do
                {
                    _directionY = ValueGenerator.Next(-MAXMOVE, MAXMOVE + 1);
                } while (!(Math.Abs(_directionY) > 2));
                if (iResult == 0)
                {
                    Result = ValueGenerator.Next(1, 7);
                }
                else
                {
                    Result = iResult;
                }
                _rollLoop = 0;
                Status = DieStatus.Rolling;
            }
            else
            {
                Status = DieStatus.Stopped;
            }
        }

        public void DrawDie()
        {
            if (Status == DieStatus.Rolling)
            {
                if ((_directionX * _directionY) > 0)
                {
                    _rotationString = "yrot.";
                    ImagePath = GetFramePicPath();

                }
                else
                {
                    _rotationString = "xrot.";
                    ImagePath = GetFramePicPath();
                }

            }
            else
            {
                Frame = (Result - 1) * 6 + _gameSettingsService.DieAngle;
                _rotationString = "stop.";
                ImagePath = GetFramePicPath();
                if (IsFixed)
                {
                    Opacity = 0.5f;
                }
                else
                {
                    Opacity = 1;
                }
            }
            NotifyPropertyChanged(nameof(Bounds));
        }

        public bool Overlapping(Die d)
        {
            return Bounds.Intersects(d.Bounds);
        }

        public void HandleCollision(Die d)
        {
            if (this.Overlapping(d))
            {
                if (Math.Abs(d.PosY - this.PosY) <= Math.Abs(d.PosX - this.PosX))
                {
                    HandleBounceX(d);
                }
                else
                {
                    HandleBounceY(d);
                }
            }
        }

        private void HandleBounceX(Die d)
        {
            Die dLeft = null;
            Die dRight = null;

            if (this.PosX < d.PosX)
            {
                dLeft = this;
                dRight = d;
            }
            else
            {
                dLeft = d;
                dRight = this;
            }

            //moving toward each other
            if (dLeft._directionX >= 0 & dRight._directionX <= 0)
            {
                this.BounceX();
                d.BounceX();
                return;
            }

            //moving right, left one caught up to right one
            if (dLeft._directionX > 0 & dRight._directionX >= 0)
            {
                dLeft.BounceX();
                return;
            }

            //moving left, right one caught up to left one
            if (dLeft._directionX <= 0 & dRight._directionX < 0)
            {
                dRight.BounceX();
            }

        }

        private void HandleBounceY(Die d)
        {
            Die dTop = null;
            Die dBot = null;

            if (this.PosY < d.PosY)
            {
                dTop = this;
                dBot = d;
            }
            else
            {
                dTop = d;
                dBot = this;
            }

            if (dTop._directionY >= 0 & dBot._directionY <= 0)
            {
                this.BounceY();
                d.BounceY();
                return;
            }

            //moving down, top one caught up to bottom one
            if (dTop._directionY > 0 & dBot._directionY >= 0)
            {
                dTop.BounceY();
                return;
            }

            //moving left, bottom one caught up to top one
            if (dTop._directionY <= 0 & dBot._directionY < 0)
            {
                dBot.BounceY();
            }

        }

        private void BounceX()
        {
            _directionX = -_directionX;
        }

        private void BounceY()
        {
            _directionY = -_directionY;
        }

        //new
        public bool ClickedOn(double x, double y)
        {
            return this.Bounds.Contains(new Point(x, y));
        }
        #endregion
    }
}
