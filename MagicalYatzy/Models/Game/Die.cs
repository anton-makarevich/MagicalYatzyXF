using Sanet.MagicalYatzy.Models.Common;
using Sanet.MagicalYatzy.Services;
using System;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Models.Game.Extensions;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.Models.Game
{
    public class Die : BindableBase
    {
        private const int MaxMove = 5;
        private const int MinDiceValue = 1;
        private const int MaxDiceValue = 6;
        private readonly IGameSettingsService _gameSettingsService;
        private readonly IValueGenerator _valueGenerator;
        private readonly IDicePanel _dicePanel;

        #region Constructor

        public Die(IDicePanel dicePanel, IGameSettingsService gameSettingsService, IValueGenerator valueGenerator)
        {
            _dicePanel = dicePanel;
            _gameSettingsService = gameSettingsService;
            _valueGenerator = valueGenerator;
        }

        #endregion

        #region Fields

        private int _rollLoop;

        //dimensions
        private const int Height = 72;
        private const int Width = 72;

        //position
        private int _posX;
        private int _posY;

        //direction
        internal int DirectionX;
        internal int DirectionY;

        private DieStatus _status = DieStatus.Stopped;

        private string _rotationString;

        private int _frame;
        private int _result = 1;

        private float _opacity;

        private string _imagePath;

        #endregion

        #region Properties

        public string StyleString => $"{_gameSettingsService.DieStyle.ToPathComponent()}";

        private int Frame
        {
            get => _frame;
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
            get => _result;
            set
            {
                if (value < MinDiceValue | value > MaxDiceValue)
                {
                    throw new Exception($"Unexpected value {value}. Should be in the range 1..6");
                }

                _result = value;
            }
        }

        internal int PosX
        {
            get => _posX;
            set
            {
                _posX = value;

                if (_posX < 0)
                {
                    _posX = 0;
                    BounceX();
                }
                
                if (_dicePanel.Bounds.Width > Width && _dicePanel.Bounds.Width < _posX + Width)
                {
                    _posX = (int) (_dicePanel.Bounds.Width) - Width;
                    BounceX();
                }
            }
        }

        internal int PosY
        {
            get => _posY;
            set
            {
                _posY = value;

                if (_posY < 0)
                {
                    _posY = 0;
                    BounceY();
                }

                if (_dicePanel.Bounds.Height > Height && _dicePanel.Bounds.Height < _posY + Height)
                {
                    _posY = (int) (_dicePanel.Bounds.Height) - Height;
                    BounceY();
                }
            }
        }

        public bool IsFixed { get; set; }

        internal DieStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                if (value != DieStatus.Stopped) return;
                DirectionX = 0;
                DirectionY = 0;
            }
        }

        public float Opacity
        {
            get => _opacity;
            private set => SetProperty(ref _opacity, value);
        }

        public string ImagePath
        {
            get => _imagePath;
            private set => SetProperty(ref _imagePath, value);
        }

        public bool IsNotRolling => Status == DieStatus.Stopped;

        public bool IsRolling => Status == DieStatus.Rolling;

        public bool IsLanding => Status == DieStatus.Landing;

        public Rectangle Bounds => new Rectangle(PosX, PosY, Width, Height);

        #endregion

        #region Methods

        private string GetFramePicPath() => StyleString + _rotationString + Frame + ".png";

        public void InitializePosition()
        {
            var width = (int)_dicePanel.Bounds.Width;
            var height = (int)_dicePanel.Bounds.Height;
    
            if (width > 0 && height > 0)
            {
                var mw = width - Width - (int)_dicePanel.SaveMargins.Right;
                if (mw < 1)
                {
                    mw = 1;
                }
                PosX = _valueGenerator.Next((int)_dicePanel.SaveMargins.Left + 1, mw);
        
                mw = height - Height - (int)_dicePanel.SaveMargins.Bottom;
                if (mw < 1)
                {
                    mw = 1;
                }
                PosY = _valueGenerator.Next((int)_dicePanel.SaveMargins.Top + 1, mw);
            }
            else
            {
                PosX = 0;
                PosY = 0;
            }

            if (PosX < 0) PosX = 0;
            if (PosY < 0) PosY = 0;
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
                    Frame += (1 * Math.Sign(DirectionY));
                    break;
                case DieStatus.Stopped:
                    return;
            }

            PosX += DirectionX;
            PosY += DirectionY;

            _rollLoop += 1;

            switch (Status)
            {
                case DieStatus.Rolling:
                    // Stop when max amount of rolls has been reached
                    if (_rollLoop > _gameSettingsService.MaxRollLoop 
                        && _valueGenerator.Next(1, GetRollDurationModifier()) < 10
                        && !IsInSaveArea())
                    {
                        Status = DieStatus.Landing;
                        _rollLoop = 0;

                        Frame = Result * 6;
                    }
                    break;
                
                case DieStatus.Landing:

                    if (_rollLoop > 5 - _gameSettingsService.DieAngle && !IsInSaveArea())
                    {
                        Status = DieStatus.Stopped;
                    }
                    break;
            }
        }

        private int GetRollDurationModifier()
        {
            var modifiers = 0;
            const int saveMarginsFactor = 10;
            if (_dicePanel.SaveMargins.Left!=0) modifiers+=saveMarginsFactor;
            if (_dicePanel.SaveMargins.Right!=0) modifiers+=saveMarginsFactor;
            if (_dicePanel.SaveMargins.Top!=0) modifiers+=saveMarginsFactor;
            if (_dicePanel.SaveMargins.Bottom!=0) modifiers+=saveMarginsFactor;
            return 100 - modifiers;
        }

        private bool IsInSaveArea()
        {
            if (_dicePanel.Bounds.Height == 0 || _dicePanel.Bounds.Width == 0) return false;
            return PosX <= _dicePanel.SaveMargins.Left
                   || PosX >= _dicePanel.Bounds.Width - Width - _dicePanel.SaveMargins.Right
                   || PosY <= _dicePanel.SaveMargins.Top
                   || PosY >= _dicePanel.Bounds.Height - Height - _dicePanel.SaveMargins.Bottom;
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
                    DirectionX = _valueGenerator.Next(-MaxMove, MaxMove + 1);
                } while (!(Math.Abs(DirectionX) > 2));
                do
                {
                    DirectionY = _valueGenerator.Next(-MaxMove, MaxMove + 1);
                } while (!(Math.Abs(DirectionY) > 2));
                Result = iResult == 0 ? _valueGenerator.Next(1, 7) : iResult;
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
                Opacity = 1;
                if ((DirectionX * DirectionY) > 0)
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
                Opacity = IsFixed? 0.5f: 1;
                ImagePath = GetFramePicPath();
            }
            NotifyPropertyChanged(nameof(Bounds));
        }

        public bool Overlapping(Die d)
        {
            return Bounds.Intersects(d.Bounds);
        }

        public void HandleCollision(Die d)
        {
            if (!Overlapping(d)) return;
            if (Math.Abs(d.PosY - PosY) <= Math.Abs(d.PosX - PosX))
                HandleBounceX(d);
            else
                HandleBounceY(d);
        }

        private void HandleBounceX(Die d)
        {
            Die dLeft;
            Die dRight;

            if (PosX < d.PosX)
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
            if (dLeft.DirectionX >= 0 & dRight.DirectionX <= 0)
            {
                BounceX();
                d.BounceX();
                return;
            }

            //moving right, left one caught up to right one
            if (dLeft.DirectionX > 0 & dRight.DirectionX >= 0)
            {
                dLeft.BounceX();
                return;
            }

            //moving left, right one caught up to left one
            if (dLeft.DirectionX <= 0 & dRight.DirectionX < 0)
            {
                dRight.BounceX();
            }
        }

        private void HandleBounceY(Die d)
        {
            Die dTop;
            Die dBot;

            if (PosY < d.PosY)
            {
                dTop = this;
                dBot = d;
            }
            else
            {
                dTop = d;
                dBot = this;
            }

            if (dTop.DirectionY >= 0 & dBot.DirectionY <= 0)
            {
                BounceY();
                d.BounceY();
                return;
            }

            //moving down, top one caught up to bottom one
            if (dTop.DirectionY > 0 & dBot.DirectionY >= 0)
            {
                dTop.BounceY();
                return;
            }

            //moving left, bottom one caught up to top one
            if (dTop.DirectionY <= 0 & dBot.DirectionY < 0)
            {
                dBot.BounceY();
            }
        }

        private void BounceX()
        {
            DirectionX = -DirectionX;
        }

        private void BounceY()
        {
            DirectionY = -DirectionY;
        }

        //new
        public bool ClickedOn(Point point)
        {
            return Bounds.Contains(point);
        }
        #endregion
    }
}