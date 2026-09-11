using System;
using Foundation;
using Sanet.MagicalYatzy.Xf.iOS.Renderers;
using Sanet.MagicalYatzy.Xf.Views.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace Sanet.MagicalYatzy.Xf.iOS.Renderers
{
    [Preserve(AllMembers = true)]
	public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView>
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public new static void Init()
		{
			// ReSharper disable once UnusedVariable
			var temp = DateTime.Now;
		}

		#region Touch Handlers

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
            Element.OnTouchesBegan(GetFirstPoint(touches));   
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
            Element.OnTouchesEnded(GetFirstPoint(touches));
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			// Ignore 
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			// Ignore 
		}

        private Point GetFirstPoint(NSSet touches) 
        {
			var touch = touches.AnyObject as UITouch;
			if (touch == null) return new Point();
			var location = touch.LocationInView(touch.View);
			return new Point(location.X, location.Y);
        }

		#endregion
	}
}