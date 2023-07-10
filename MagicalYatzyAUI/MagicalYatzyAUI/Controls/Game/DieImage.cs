using System.ComponentModel;
using Avalonia.Controls;
using Sanet.MagicalYatzy.Avalonia.Helpers;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Game;

public class DieImage : Image
{
    private readonly Die _die;

    public DieImage(Die die)
    {
        _die = die;
        die.PropertyChanged += OnDiePropertyChanged;

        UpdateImage();
        UpdatePosition();
    }

    private void OnDiePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_die.Bounds):
                UpdatePosition();
                break;
            case nameof(_die.ImagePath):
            case nameof(_die.Opacity):
                UpdateImage();
                break;
        }
    }

    private void UpdatePosition()
    {
        Canvas.SetLeft(this, _die.Bounds.Left);
        Canvas.SetTop(this, _die.Bounds.Top);
        Width = _die.Bounds.Width;
        Height = _die.Bounds.Height;
    }

    private void UpdateImage()
    {
        Source = null;

        if (!string.IsNullOrWhiteSpace(_die.ImagePath))
        {
            var source = DiceLoaderHelper.GetDiceImageByPath(_die.ImagePath);
            if (source != null)
                Source =source;
        }

        Opacity = _die.Opacity;
    }
    
    public Die Die => _die;
}