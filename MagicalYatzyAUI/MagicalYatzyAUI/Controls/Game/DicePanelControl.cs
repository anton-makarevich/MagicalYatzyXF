﻿using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Views;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Game
{
    public class DicePanelControl: Canvas, IDicePanelView
    {
        private readonly bool _hasTapRecognizer;
        private IDicePanel? _dicePanelModel;

        public DicePanelControl(bool addTapRecognizer = false)
        {
            _hasTapRecognizer = addTapRecognizer;
        }

        public IDicePanel? DicePanel
        {
            get => _dicePanelModel;
            set
            {
                if (_dicePanelModel != null)
                {
                    _dicePanelModel.DieAdded -= OnDieAdded;
                    _dicePanelModel.DieRemoved -= OnDieRemoved;
                }
                _dicePanelModel = value;
                
                if (_dicePanelModel != null)
                {
                    _dicePanelModel.DieAdded += OnDieAdded;
                    _dicePanelModel.DieRemoved += OnDieRemoved;
                }
            }
        }

        private void OnDieRemoved(object? sender, Die e)
        {
            var dieImageToRemove = Children.FirstOrDefault(c => (c as DieImage)?.Die == e);
            if (dieImageToRemove != null)
            {
                Children.Remove(dieImageToRemove);
            }
        }

        private void OnDieAdded(object? sender, Die die)
        {
            Children.Insert(0, new DieImage(die));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var arrangedSize = base.ArrangeOverride(finalSize);

            if (finalSize is { Width: > 0, Height: > 0 })
                _dicePanelModel?.Resize((int)finalSize.Width, (int)finalSize.Height);

            return arrangedSize;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (!_hasTapRecognizer) return;
            var point = new Models.Common.Point(e.GetPosition(this).X, e.GetPosition(this).Y);
            _dicePanelModel?.DieClicked(point);
            base.OnPointerReleased(e);
        }
        
        public Thickness SaveMargins
        {
            get => _dicePanelModel == null 
                ? new Thickness() 
                : new Thickness(_dicePanelModel.SaveMargins.Left, _dicePanelModel.SaveMargins.Top, _dicePanelModel.SaveMargins.Right, _dicePanelModel.SaveMargins.Bottom);
            set
            {
                if (_dicePanelModel != null)
                    _dicePanelModel.SaveMargins =
                        new Models.Common.Thickness(value.Left, value.Top, value.Right, value.Bottom);
            }
        }
    }
}
