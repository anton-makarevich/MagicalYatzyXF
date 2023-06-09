using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace Sanet.MagicalYatzy.Avalonia.Extensions;

public static class AnimationExtensions
{
    private static bool _isRunning;

    public static async Task AnimateClick(this Control control)
    {
        if (_isRunning)
            return;

        _isRunning = true;
        var size = new Size(300,250);
        const double scale = 0.5;
        var scaledSize = size * scale;
        await control.ScaleTo(size, scaledSize, TimeSpan.FromMilliseconds(200), new QuadraticEaseIn());
        await control.ScaleTo(scaledSize, size, TimeSpan.FromMilliseconds(200), new QuadraticEaseOut());
        _isRunning = false;
    }

    private static async Task ScaleTo(this Control control, Size  from, Size to, TimeSpan duration, Easing? easing = null)
    {
        easing ??= new LinearEasing();
        
        var framerate= TimeSpan.FromSeconds(1 / 60.0);
        
        var totalTicks = duration.TotalMilliseconds / framerate.TotalMilliseconds;
        for (var currentTick = 0;currentTick<totalTicks; currentTick++)
        {
            var progress = currentTick / totalTicks;
            var currentSize = from+ (to - from) * easing.Ease(progress);
     
            // Calculate the new position of the top-left corner based on the center position
            var newX = control.Bounds.X + (control.Width - currentSize.Width) / 2.0;
            var newY = control.Bounds.Y + (control.Height - currentSize.Height) / 2.0;

            // Update the width, height, and position of the control
            control.Width = currentSize.Width;
            control.Height = currentSize.Height;
            control.SetValue(Canvas.LeftProperty, newX);
            control.SetValue(Canvas.TopProperty, newY);
            await Task.Delay(framerate);
        }
    }
}