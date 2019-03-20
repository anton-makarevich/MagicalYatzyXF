using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
    }
}
