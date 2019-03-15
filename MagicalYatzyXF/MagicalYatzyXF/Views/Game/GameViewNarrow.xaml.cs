using Xamarin.Forms.Xaml;

namespace Sanet.MagicalYatzy.XF.Views.Game
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameViewNarrow : GameView
    {
        public GameViewNarrow()
        {
            InitializeComponent();
        }

        protected override void InitDicePanel()
        {
            
        }
    }
}
