using Autofac;

namespace Sanet.MagicalYatzy.XF.UWP
{
    public sealed partial class MainPage
    {
        private IContainer _container;
        public MainPage()
        {
            this.InitializeComponent();
            var builder = new ContainerBuilder();
            builder.RegisterModule<PlatformModule>();
            _container = builder.Build();
            LoadApplication(new Sanet.MagicalYatzy.XF.App(_container));
        }
    }
}
