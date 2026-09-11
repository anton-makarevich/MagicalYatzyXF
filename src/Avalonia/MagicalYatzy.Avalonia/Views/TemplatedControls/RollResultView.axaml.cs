using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Sanet.MagicalYatzy.Avalonia.Views.TemplatedControls;

public class RollResultView : TemplatedControl
{
    public static readonly StyledProperty<int> ScoreValueProperty = AvaloniaProperty.Register<RollResultView, int>(
        nameof(ScoreValue));

    public static readonly StyledProperty<string> ScoreNameProperty = AvaloniaProperty.Register<RollResultView, string>(
        nameof(ScoreName));

    public static readonly StyledProperty<double> ScoreFontSizeProperty = AvaloniaProperty.Register<RollResultView, double>(
        nameof(ScoreFontSize));

    public double ScoreFontSize
    {
        get => GetValue(ScoreFontSizeProperty);
        set => SetValue(ScoreFontSizeProperty, value);
    }
    
    public string ScoreName
    {
        get => GetValue(ScoreNameProperty);
        set => SetValue(ScoreNameProperty, value);
    }
    
    public int ScoreValue
    {
        get => GetValue(ScoreValueProperty);
        set => SetValue(ScoreValueProperty, value);
    }
}