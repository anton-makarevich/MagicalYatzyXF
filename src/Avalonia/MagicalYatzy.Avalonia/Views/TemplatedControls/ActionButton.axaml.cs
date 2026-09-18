using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Sanet.MagicalYatzy.Avalonia.Extensions;

namespace Sanet.MagicalYatzy.Avalonia.Views.TemplatedControls;

public class ActionButton : Button
{
    public static readonly StyledProperty<string> ImageSourceProperty = AvaloniaProperty.Register<ActionButton, string>(
        nameof(ImageSource));

    public static readonly DirectProperty<ActionButton, bool> HasLabelProperty =
        AvaloniaProperty.RegisterDirect<ActionButton, bool>(nameof(HasLabel), o => o.HasLabel);

    public string ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public bool HasLabel
    {
        get;
        private set => SetAndRaise(HasLabelProperty, ref field, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsEnabled)
            _ = this.AnimateClick();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            HasLabel = change.NewValue != null;
        }
    }
}
