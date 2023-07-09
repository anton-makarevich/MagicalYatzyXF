using Avalonia.Markup.Xaml;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Views.Avalonia;

namespace Sanet.MagicalYatzy.Avalonia.Views;

public partial class GameResultsView : BaseView<GameResultsViewModel>
{
    public GameResultsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}