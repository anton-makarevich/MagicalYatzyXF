using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MagicalYatzy.Xf.Views.Controls.TabControl;
using Sanet.MagicalYatzy.Xf.Views.Fragments;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;

namespace Sanet.MagicalYatzy.Xf.Views.Game
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
            _diceGrid.Children.Add(DicePanelView);
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.Game.ResultApplied += Game_ResultApplied;
            ViewModel.DicePanel.RollStarted += DicePanelOnRollStarted;
            if (ViewModel.DicePanel.IsRolling)
                DicePanelOnRollStarted(this, null);
        }

        void Game_ResultApplied(object sender, MagicalYatzy.Models.Events.RollResultEventArgs e)
        {
            TabBar.Activate(TabBar.TabChildren.First(), true);
        }
        
        protected override void OnDisappearing()
        {
            ViewModel.Game.ResultApplied -= Game_ResultApplied;
            ViewModel.DicePanel.RollStarted -= DicePanelOnRollStarted;
            base.OnDisappearing();
        }

        void DicePanelOnRollStarted(object sender, System.EventArgs e)
        {
            TabBar.Activate(TabBar.TabChildren.Last(), true);
        }
    }
}
