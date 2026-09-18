using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Sanet.MagicalYatzy.Avalonia.Extensions;

public static class AnimationExtensions
{
    private const double PressedScale = 0.5;
    private static readonly ConditionalWeakTable<Control, Task> RunningAnimations = new();

    private static readonly Animation ShrinkAnimation = new()
    {
        Duration = TimeSpan.FromMilliseconds(200),
        Easing = new QuadraticEaseIn(),
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0d),
                Setters =
                {
                    new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1.0 },
                    new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1.0 }
                }
            },
            new KeyFrame
            {
                Cue = new Cue(1d),
                Setters =
                {
                    new Setter { Property = ScaleTransform.ScaleXProperty, Value = PressedScale },
                    new Setter { Property = ScaleTransform.ScaleYProperty, Value = PressedScale }
                }
            }
        }
    };

    private static readonly Animation GrowAnimation = new()
    {
        Duration = TimeSpan.FromMilliseconds(200),
        Easing = new QuadraticEaseOut(),
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0d),
                Setters =
                {
                    new Setter { Property = ScaleTransform.ScaleXProperty, Value = PressedScale },
                    new Setter { Property = ScaleTransform.ScaleYProperty, Value = PressedScale }
                }
            },
            new KeyFrame
            {
                Cue = new Cue(1d),
                Setters =
                {
                    new Setter { Property = ScaleTransform.ScaleXProperty, Value = 1.0 },
                    new Setter { Property = ScaleTransform.ScaleYProperty, Value = 1.0 }
                }
            }
        }
    };

    public static async Task AnimateClick(this Control control)
    {
        if (RunningAnimations.TryGetValue(control, out var running) && !running.IsCompleted)
            return;

        var scaleTransform = GetOrCreateScaleTransform(control);
        var task = RunClickAnimation(scaleTransform);
        RunningAnimations.AddOrUpdate(control, task);
        await task;
        RunningAnimations.Remove(control);
    }

    private static async Task RunClickAnimation(ScaleTransform scaleTransform)
    {
        await ShrinkAnimation.RunAsync(scaleTransform);
        await GrowAnimation.RunAsync(scaleTransform);
    }

    private static ScaleTransform GetOrCreateScaleTransform(Control control)
    {
        if (control.RenderTransform is ScaleTransform scaleTransform)
            return scaleTransform;

        if (control.RenderTransform is TransformGroup group)
        {
            foreach (var transform in group.Children)
            {
                if (transform is ScaleTransform existing)
                    return existing;
            }
        }

        scaleTransform = new ScaleTransform();
        if (control.RenderTransform is TransformGroup existingGroup)
        {
            existingGroup.Children.Add(scaleTransform);
        }
        else
        {
            control.RenderTransform = scaleTransform;
        }

        return scaleTransform;
    }
}
