using System;
using AppKit;
using Foundation;
using Sanet.MagicalYatzy.XF.Mac.Renderers;
using Sanet.MagicalYatzy.XF.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]
namespace Sanet.MagicalYatzy.XF.Mac.Renderers
{
    [Preserve(AllMembers = true)]
	public class TappableContentViewRenderer : VisualElementRenderer<TappableContentView>
	{
		/// <summary>
		/// Used for registration with dependency service
		/// </summary>
		public static void Init()
		{
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
            return theEvent.LocationInWindow.ToPoint();
        }

		#endregion
	}
}

