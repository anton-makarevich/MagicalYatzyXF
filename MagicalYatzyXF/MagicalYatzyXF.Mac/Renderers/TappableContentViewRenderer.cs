using System;
using AppKit;
using Foundation;
using MagicalYatzyXF.Mac.Renderers;
using Sanet.MagicalYatzy.Xf.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace MagicalYatzyXF.Mac.Renderers
{
    [Preserve(AllMembers = true)]
	public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView>
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		// ReSharper disable once UnusedMember.Global
		public static void Init()
		{
			// ReSharper disable once UnusedVariable
			var temp = DateTime.Now;
		}

        #region Touch Handlers

        public override void MouseDown(NSEvent theEvent)
        {
            Element.OnTouchesBegan(GetFirstPoint(theEvent));
        }

        public override void MouseUp(NSEvent theEvent)
        {
            Element.OnTouchesEnded(GetFirstPoint(theEvent));
        }

        private Point GetFirstPoint(NSEvent theEvent) 
        {
             var locationInView = ConvertPointFromView(theEvent.LocationInWindow, null);
             return new Point(locationInView.X, Bounds.Height - locationInView.Y);
        }

		#endregion
	}
}