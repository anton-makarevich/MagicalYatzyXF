using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MagicalYatzy.XF.Views.Controls.TabControl;
using Sanet.MagicalYatzy.XF.Views.Fragments;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameViewNarrow
    {
        private readonly Grid _diceGrid = new Grid();
        public GameViewNarrow()
        {
            InitializeComponent();
        }

        protected override void InitDicePanel()
        {
            base.InitDicePanel();
            _diceGrid.Children.Add(DicePanel);
        }

        protected override void OnViewModelSet()
        {
            _diceGrid.BackgroundColor = Color.Blue;
            TabBar.TabChildren.Add(new TabItem(ViewModel.ScoresTitle, new ResultsTable()));
            TabBar.TabChildren.Add(new TabItem(ViewModel.PanelTitle, _diceGrid));
        }
        
        private void RollResultSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem is RollResultViewModel viewModel)
                ViewModel.ApplyRollResult(viewModel.RollResult);
        }
    }
}
