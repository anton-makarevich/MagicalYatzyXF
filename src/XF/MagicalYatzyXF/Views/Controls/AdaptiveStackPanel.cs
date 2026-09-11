using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Controls
{
    public class AdaptiveStackPanel: StackLayout
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            var isHorizontal = width > height;
            Orientation = isHorizontal ? StackOrientation.Horizontal : StackOrientation.Vertical;
            foreach (var child in Children)
                child.WidthRequest = isHorizontal ? width : width * 0.5;
            base.OnSizeAllocated(width, height);
        }
    }
}
