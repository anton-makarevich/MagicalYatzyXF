using System;
using Avalonia.Input;
using Sanet.MagicalYatzy.Models.Common;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Interactions
{
    public class DicePanelTapRecognizer:TappableContentView
    {
        public event EventHandler<Point> Tapped;

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            Tapped?.Invoke(this,new Point(e.GetPosition(this).X,e.GetPosition(this).Y));
            base.OnPointerReleased(e);
        }
    }
}