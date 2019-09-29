using System;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Controls.Game
{
    public class DicePanelTapRecognizer:TappableContentView
    {
        public event EventHandler<Point> Tapped; 
        public override void OnTouchesBegan(Point point)
        {
            Tapped?.Invoke(this,point);
        }

        public override void OnTouchesEnded(Point point)
        {
        }
    }
}