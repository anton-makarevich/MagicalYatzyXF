using AppKit;
using MagicalYatzyXF.Mac.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly:ExportRenderer(typeof(ListView), typeof(FixedListViewRenderer))]
namespace MagicalYatzyXF.Mac.Renderers
{
	public class FixedListViewRenderer:ListViewRenderer
    {
		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(Color.Transparent);
			if (NativeTableView == null) return;

			if (Control is NSScrollView scroller)
			{
				scroller.HasVerticalScroller = false;
				scroller.DrawsBackground = false;
				scroller.ContentView.BackgroundColor = NSColor.Clear;
			}
			NativeTableView.GridColor = NSColor.Clear; //color.ToNSColor(NSColor.Red);
			NativeTableView.BackgroundColor = NSColor.Clear;
			NativeTableView.HeaderView = null;
		}
	}
}