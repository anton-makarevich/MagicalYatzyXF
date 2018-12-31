using System.Linq;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Controls.TabControl
{
    public class StackLayoutExtended: StackLayout
    {
        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            var total = Children.Sum(t => t.Width);
            var parentWidth = (Parent as View).Width;

            if (total < parentWidth)
            {
                // We need more space
                var diff = (parentWidth - total) / Children.Count;

                var xoffset = 0.0;
                foreach (var child in Children)
                {
                    child.Layout(new Rectangle(child.X + xoffset, child.Y, child.Width + diff, child.Height));
                    xoffset += diff;
                }
            }
        }
    }
}
