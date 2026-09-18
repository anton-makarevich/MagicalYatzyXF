using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Sanet.MagicalYatzy.Avalonia.Helpers;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Game;

public class DieImage : Image
{
    private bool _isSubscribed;

    public DieImage(Die die)
    {
        Die = die;
        Subscribe();

        UpdateImage();
        UpdatePosition();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Subscribe();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        base.OnDetachedFromVisualTree(e);
    }

    public void Detach()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (_isSubscribed) return;
        Die.PropertyChanged += OnDiePropertyChanged;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;
        Die.PropertyChanged -= OnDiePropertyChanged;
        _isSubscribed = false;
    }

    private void OnDiePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Die.Bounds):
                UpdatePosition();
                break;
            case nameof(Die.ImagePath):
            case nameof(Die.Opacity):
                UpdateImage();
                break;
        }
    }

    private void UpdatePosition()
    {
        Canvas.SetLeft(this, Die.Bounds.Left);
        Canvas.SetTop(this, Die.Bounds.Top);
        Width = Die.Bounds.Width;
        Height = Die.Bounds.Height;
    }

    private void UpdateImage()
    {
        Source = null;

        if (!string.IsNullOrWhiteSpace(Die.ImagePath))
        {
            var source = DiceLoaderHelper.GetDiceImageByPath(Die.ImagePath);
            if (source != null)
                Source =source;
        }

        Opacity = Die.Opacity;
    }
    
    public Die Die { get; }
}