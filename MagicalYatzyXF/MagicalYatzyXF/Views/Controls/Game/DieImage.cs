using System.ComponentModel;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.XF.Extensions;
using Sanet.MagicalYatzy.XF.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls.Game
{
    public class DieImage : ContentView
    {
        private readonly Die _die;
        SKCanvasView _canvasView;
        public DieImage(Die die)
        {
            _die = die;
            die.PropertyChanged += OnDiePropertyChanged;
            _canvasView = new SKCanvasView();
            _canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = _canvasView;
            UpdateImage();
            UpdatePosition();
        }

        private void OnDiePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_die.Bounds))
            {
                UpdatePosition();
            }
            else if (e.PropertyName == nameof(_die.ImagePath))
            {
                UpdateImage();
            }
        }

        private void UpdatePosition()
        {
            AbsoluteLayout.SetLayoutBounds(this, _die.Bounds.ToFormsRectangle());
        }

        private void UpdateImage()
        {
            _canvasView.InvalidateSurface();
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_die.ImagePath))
                _die.DrawDie();
            var source = DiceLoaderHelper.GetDiceImageByPath(_die.ImagePath);

            if (source == null)
                return;

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.DrawBitmap(source,
                    new SKRect(0, 0, info.Width, info.Height));
        }
    }
}
