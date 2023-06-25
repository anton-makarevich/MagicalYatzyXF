using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Sanet.MagicalYatzy.Avalonia.Views.TemplatedControls;

public class ActionButton : Button
{
    public static readonly StyledProperty<string> ImageSourceProperty = AvaloniaProperty.Register<ActionButton, string>(
        nameof(ImageSource));

    public string ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }
}