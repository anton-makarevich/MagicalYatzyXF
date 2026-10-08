using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Sanet.MVVM.Core.Views;

namespace Sanet.MagicalYatzy.Avalonia.Views.Settings;

public partial class AddHubView : UserControl, IBaseView
{
    public AddHubView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public object? ViewModel
    {
        get => DataContext;
        set => DataContext = value;
    }
}
