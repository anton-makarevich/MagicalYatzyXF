using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace Sanet.MagicalYatzy.Avalonia.Views.Fragments;

public partial class RollResultsPanel : UserControl
{
    public RollResultsPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<GameButtons, Orientation>(
        nameof(Orientation));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}