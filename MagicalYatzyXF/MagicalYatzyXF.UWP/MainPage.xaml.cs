using SimpleInjector;

namespace Sanet.MagicalYatzy.XF.UWP
{
    public sealed partial class MainPage
    {
        private Container _container;
        public MainPage()
        {
            this.InitializeComponent();
            _container = new Container();
            _container.RegisterModules();
            LoadApplication(new Sanet.MagicalYatzy.XF.App(_container));
        }
    }
}
