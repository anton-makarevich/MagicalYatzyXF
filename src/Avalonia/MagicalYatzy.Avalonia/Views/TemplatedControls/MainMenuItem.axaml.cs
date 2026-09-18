using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Sanet.MagicalYatzy.Avalonia.Extensions;

namespace Sanet.MagicalYatzy.Avalonia.Views.TemplatedControls;

public class MainMenuItem : TemplatedControl
{
    public static readonly StyledProperty<string> CaptionProperty = AvaloniaProperty.Register<MainMenuItem, string>(
        nameof(Caption),"Caption");

    public string Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<MainMenuItem, string>(
        nameof(Description), "description");
                                                     
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<string> ImageSourceProperty = AvaloniaProperty.Register<MainMenuItem, string>(
        nameof(ImageSource),"SanetDice.png");

    public string ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsEnabled)
            _ = this.AnimateClick();
    }
}