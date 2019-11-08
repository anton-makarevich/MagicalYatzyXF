using System;
using SimpleInjector;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Application = Gtk.Application;

namespace Sanet.MagicalYatzy.Xf.Gtk
{
    class MainClass
    {
        private static Container _container;
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();
            Forms.Init();
            
            _container = new Container();
            _container.RegisterModules();
            App.Container = _container;
            
            var app = new App();
            var win = new FormsWindow();
            
            win.LoadApplication(app);
            win.SetApplicationTitle("MagicalYatzyXF.GTK");
            
            win.Show();
            Application.Run();
        }
    }
}
