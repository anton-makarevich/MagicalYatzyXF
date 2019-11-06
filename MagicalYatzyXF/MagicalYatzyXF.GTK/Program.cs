using System;
using Sanet.MagicalYatzy.Xf;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Application = Gtk.Application;

namespace MagicalYatzyXF.GTK
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();
            Forms.Init();
            var app = new App();
            var win = new FormsWindow();
            
            win.LoadApplication(app);
            win.SetApplicationTitle("MagicalYatzyXF.GTK");
            
            win.Show();
            Application.Run();
        }
    }
}
