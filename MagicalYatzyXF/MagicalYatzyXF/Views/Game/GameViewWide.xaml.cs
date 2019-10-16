using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.Xf.Views.Controls;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.Xf.Views.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameViewWide 
    {
        public GameViewWide()
        {
            InitializeComponent();
        }

        protected override void InitDicePanel()
        {
            base.InitDicePanel();
            DicePanelView.SetValue(Grid.RowProperty, 1);
            DicePanelView.SetValue(Grid.ColumnProperty, 1);
            PageGrid.Children.Insert(0, DicePanelView);
        }

        public void RollResultSelected(object sender, SelectEventArgs e)
        {
            if (e?.ItemData is RollResultViewModel viewModel)
                ViewModel.ApplyRollResult(viewModel.RollResult);
        }
    }
}
