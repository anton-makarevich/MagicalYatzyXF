﻿using Xamarin.Forms;
namespace Sanet.MagicalYatzy.XF.Controls
{
    public class AdaptiveStackPanel: StackLayout
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            var isHorizontal = width > height;
            Orientation = isHorizontal ? StackOrientation.Horizontal : StackOrientation.Vertical;
            foreach (var child in this.Children)
                child.WidthRequest = isHorizontal ? width : width * 0.5;
            base.OnSizeAllocated(width, height);
        }
    }
}