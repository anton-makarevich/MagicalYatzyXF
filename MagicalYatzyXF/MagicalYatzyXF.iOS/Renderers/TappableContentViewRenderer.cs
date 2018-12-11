using System;
using Foundation;
using Sanet.MagicalYatzy.XF.Controls;
using Sanet.MagicalYatzy.XF.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace Sanet.MagicalYatzy.XF.iOS.Renderers
{
    [Preserve(AllMembers = true)]
	public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView>
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public new static void Init()
		{
			var temp = DateTime.Now;
		}

		#region Touch Handlers

		public override void TouchesBegan(Foundation.NSSet touches, UIEvent evt)
		{
            Element.OnTouchesBegan(GetFirstPoint(touches));
		    
		}

		public override void TouchesEnded(Foundation.NSSet touches, UIEvent evt)
		{
            Element.OnTouchesEnded(GetFirstPoint(touches));
		}

		public override void TouchesCancelled(Foundation.NSSet touches, UIEvent evt)
		{
			// Ignore 
		}

		public override void TouchesMoved(Foundation.NSSet touches, UIEvent evt)
		{
			// Ignore 
		}

        private Point GetFirstPoint(NSSet touches) 
        {
			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				var posc = touch.LocationInView(touch.View);
				return new Point(posc.X, posc.Y);
			}
            return new Point();
        }

		#endregion
	}
}

