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
            Sanet.MagicalYatzy.Xf.App.Container = _container;
            LoadApplication(new Sanet.MagicalYatzy.Xf.App());
        }
    }
}
