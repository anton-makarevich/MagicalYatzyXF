using Avalonia;
using Avalonia.Controls;
using Sanet.MagicalYatzy.Avalonia.Controls.Interactions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Views;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Game
{
    public class DicePanelControl: Canvas, IDicePanelView
    {
        private IDicePanel _dicePanelModel;
        private readonly TapRecognizer? _tapRecognizer;

        public DicePanelControl(bool addTapRecognizer = false)
        {
            if (!addTapRecognizer) return;
            _tapRecognizer = new TapRecognizer();
            _tapRecognizer.Tapped += (sender, point) =>
            {
                _dicePanelModel?.DieClicked(point);
            };
            Children.Add(_tapRecognizer);
        }

        public IDicePanel DicePanel
        {
            get => _dicePanelModel;
            set
            {
                // TODO clear handlers
                _dicePanelModel = value;
                
                // TODO add handlers
                _dicePanelModel.DieAdded += OnDieAdded;
            }
        }

        private void OnDieAdded(object sender, Die die)
        {
            Children.Insert(0, new DieImage(die));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_tapRecognizer != null)
            {
                _tapRecognizer.Width = finalSize.Width;
                _tapRecognizer.Height = finalSize.Height;
            }

            var arrangedSize = base.ArrangeOverride(finalSize);

            if (finalSize is { Width: > 0, Height: > 0 })
                _dicePanelModel.Resize((int)finalSize.Width, (int)finalSize.Height);

            return arrangedSize;
        }
    }
}
