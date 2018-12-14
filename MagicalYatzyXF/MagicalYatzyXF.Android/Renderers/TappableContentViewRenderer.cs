using Xamarin.Forms;
using Sanet.MagicalYatzy.XF.Controls;
using Sanet.MagicalYatzy.XF.Droid.Renderers;
using Xamarin.Forms.Platform.Android;
using Android.Views;
using System;
using System.Collections.Generic;
using Android.Content;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace Sanet.MagicalYatzy.XF.Droid.Renderers
{
    public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView>
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public static void Init()
		{
			var temp = DateTime.Now;
		}

        public TappableContentViewRenderer(Context context): base(context) { }

        #region Touch Handlers

        public override bool OnTouchEvent(MotionEvent e)
		{
            if (e.PointerCount == 0 || Element == null)
                return false;
            
			var scale = Element.Width / Width;

			var touchInfo = new List<Point>();
			var coord = new MotionEvent.PointerCoords();
			e.GetPointerCoords(0, coord);
			var point = new Point(coord.X * scale, coord.Y * scale);

            // Handle touch actions
			switch (e.Action)
            {
				case MotionEventActions.Down:
					Element.OnTouchesBegan(point);
					break;

                case MotionEventActions.Up:
                    Element.OnTouchesEnded(point);
                    break;

	            //TODO add more action types
			}

            return true;
		}

		#endregion
	}
}