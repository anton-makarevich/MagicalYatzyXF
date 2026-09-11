namespace Sanet.MagicalYatzy.Models.Common;

public struct Thickness
{
    public Thickness()
    {
    }

    public Thickness(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
    
    public Thickness(double value)
    {
        Left = value;
        Top = value;
        Right = value;
        Bottom = value;
    }
    
    public Thickness(double horizontal, double vertical)
    {
        Left = horizontal;
        Top = vertical;
        Right = horizontal;
        Bottom = vertical;
    }
    
    public double Left { get; set; } = 0.0;
    public double Right { get; set; } = 0.0;
    public double Top { get; set; } = 0.0;
    public double Bottom { get; set; } = 0.0;
}