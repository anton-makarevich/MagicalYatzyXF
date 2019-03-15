using Sanet.MagicalYatzy.XF.Views.Controls.Game;
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
            if (DicePanel != null)
                return;

            DicePanel = new DicePanelXF
            {
                InputTransparent = Device.RuntimePlatform == Device.macOS
            };

            DicePanel.SetValue(Grid.RowProperty, 1);
            DicePanel.SetValue(Grid.ColumnProperty, 1);
            DicePanel.DicePanel = ViewModel.DicePanel;
            PageGrid.Children.Insert(0, DicePanel);
            ViewModel.DicePanel.DiceCount = 5;
            ViewModel.DicePanel.RollDelay = 30;
        }
    }
}
