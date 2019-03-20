using Sanet.MagicalYatzy.XF.Views.Controls.TabControl;
using Sanet.MagicalYatzy.XF.Views.Fragments;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameViewNarrow : GameView
    {
        private Grid diceGrid = new Grid();
        public GameViewNarrow()
        {
            InitializeComponent();
        }

        protected override void InitDicePanel()
        {
            base.InitDicePanel();
            diceGrid.Children.Add(DicePanel);
        }

        protected override void OnViewModelSet()
        {
            diceGrid.BackgroundColor = Color.Blue;
            TabBar.TabChildren.Add(new TabItem(ViewModel.ScoresTitle, new ResultsTable()));
            TabBar.TabChildren.Add(new TabItem(ViewModel.PanelTitle, diceGrid));
        }
    }
}
