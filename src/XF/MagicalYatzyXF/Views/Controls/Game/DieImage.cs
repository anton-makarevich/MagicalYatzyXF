using System.ComponentModel;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Xf.Extensions;
using Sanet.MagicalYatzy.Xf.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Controls.Game
{
    public class DieImage : ContentView
    {
        private readonly Die _die;
        readonly SKCanvasView _canvasView;
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
            else if (e.PropertyName == nameof(_die.ImagePath) || e.PropertyName == nameof(_die.Opacity))
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

            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;

            canvas.Clear();
            
            using (var paint = new SKPaint())
            {
                var rect = new SKRect(0, 0, info.Width, info.Height);
                paint.Color = paint.Color.WithAlpha((byte)(_die.Opacity*255));
                canvas.DrawBitmap(source, rect, paint);
            }
        }
    }
}
