using Sanet.MagicalYatzy.XF.Extensions;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Controls
{
    public class TappableContentView : ContentView
    {
	    private DateTime _startTapTime;

		private readonly int _tapTime = 1500;

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TappableContentView), null);

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TappableContentView), null);
        
		public object CommandParameter
		{
			get { return GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

        public virtual void OnTouchesBegan(Point point)
        {
            if (IsEnabled)
            {
                _startTapTime = DateTime.Now;
                this.AnimateClick();
            }
        }

        public virtual void OnTouchesEnded(Point point)
        {
            if (IsEnabled)
            {
                ProceedTap();
            }
        }

        protected virtual void ProceedTap()
        {
            var tapTime = (DateTime.Now - _startTapTime).TotalMilliseconds;
            if (tapTime < _tapTime)
            {
                Command?.Execute(CommandParameter);
            }
        }
    }
}
