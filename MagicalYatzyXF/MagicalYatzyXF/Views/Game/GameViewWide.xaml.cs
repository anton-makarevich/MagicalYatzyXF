using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sanet.MagicalYatzy.XF.Views.Controls;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.XF.Views.Game
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
            DicePanel.SetValue(Grid.RowProperty, 1);
            DicePanel.SetValue(Grid.ColumnProperty, 1);
            PageGrid.Children.Insert(0, DicePanel);
        }

        public void RollResultSelected(object sender, SelectEventArgs e)
        {
            if (e?.ItemData is RollResultViewModel viewModel)
                ViewModel.ApplyRollResult(viewModel.RollResult);
        }
    }
}
