using Sanet.MagicalYatzy.Models.Game;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls.Game
{
    public class DicePanelXf: AbsoluteLayout
    {
        private IDicePanel _dicePanelModel;

        public DicePanelXf()
        {
            BackgroundColor = Color.Black;
        }

        public IDicePanel DicePanel
        {
            get { return _dicePanelModel; }
            set
            {
                // TODO clear handlers
                _dicePanelModel = value;
                
                // TODO add handlers
                _dicePanelModel.DieAdded += OnDieAdded;
            }
        }

        private void OnDieAdded(object sender, Die die)
        {
            Children.Add(new DieImage(die));
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);
            if (width > 0 && height > 0)
                _dicePanelModel.Resize((int)width, (int)height);
        }
    }
}
