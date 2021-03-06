﻿using AppKit;
using Foundation;
using SimpleInjector;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Sanet.MagicalYatzy.Xf.Mac
{
    [Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
    {
        private readonly NSWindow _window;
		private Container _container;
        public AppDelegate()
        {
			var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            
            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "MagicalYatzy",
                TitleVisibility = NSWindowTitleVisibility.Hidden
            };
        }

		public override NSWindow MainWindow => _window;

        public override void DidFinishLaunching(NSNotification notification)
        {
			Forms.Init();

            _container = new Container();
            _container.RegisterModules();
            App.Container = _container;
            LoadApplication(new App());

            base.DidFinishLaunching(notification);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
