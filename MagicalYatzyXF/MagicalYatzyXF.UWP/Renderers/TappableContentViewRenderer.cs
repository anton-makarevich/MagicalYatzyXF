using Sanet.MagicalYatzy.XF.Controls;
using Sanet.MagicalYatzy.XF.UWP.Renderers;
using System;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace Sanet.MagicalYatzy.XF.UWP.Renderers
{
    public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView, Grid>
    {
        /// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public  static void Init()
        {
            var temp = DateTime.Now;
        }

        public TappableContentViewRenderer():base()
        {
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Element == null)
                return;

            var p = e.GetCurrentPoint(this);
            Element.OnTouchesEnded(new Xamarin.Forms.Point(p.Position.X, p.Position.Y));
        }

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if ( Element == null)
                return;

            var p = e.GetCurrentPoint(this);
            Element.OnTouchesBegan(new Xamarin.Forms.Point(p.Position.X, p.Position.Y));
        }

        protected override void Dispose(bool disposing)
        {
            PointerPressed -= OnPointerPressed;
            PointerReleased += OnPointerReleased;
            base.Dispose(disposing);
        }
    }
}
