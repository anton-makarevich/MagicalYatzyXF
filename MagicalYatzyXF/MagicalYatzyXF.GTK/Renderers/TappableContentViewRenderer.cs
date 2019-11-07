using Gtk;
using Sanet.MagicalYatzy.Xf.Gtk.Renderers;
using Sanet.MagicalYatzy.Xf.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(TappableContentView), typeof(TappableContentViewRenderer))]

namespace Sanet.MagicalYatzy.Xf.Gtk.Renderers
{
    public class TappableContentViewRenderer :ViewRenderer<TappableContentView, Fixed>
    {
        private Fixed _fixed;
        
        protected override void OnElementChanged(ElementChangedEventArgs<TappableContentView> e)
        {
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    if(_fixed == null)
                    {
                        _fixed = new Fixed();
                    }

                    SetNativeControl(_fixed);
                }
            }

            base.OnElementChanged(e);
        }
    }
}
