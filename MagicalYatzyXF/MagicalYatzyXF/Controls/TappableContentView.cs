using Sanet.MagicalYatzy.XF.Extensions;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Controls
{
    public class TappableContentView : ContentView
    {
		private bool _isPressed;
		private DateTime _startTapTime;

		private int _tapTime = 1500;

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

        public void OnTouchesBegan(Point position)
        {
            if (IsEnabled)
            {
                _startTapTime = DateTime.Now;
                _isPressed = true;
                this.AnimateClick();
            }
        }

        public void OnTouchesEnded(Point point)
        {
            if (IsEnabled)
            {
                ProceedTap();
            }
        }

        protected virtual void ProceedTap()
        {
            _isPressed = false;
            var tapTime = (DateTime.Now - _startTapTime).TotalMilliseconds;
            if (tapTime < _tapTime)
            {
                Command?.Execute(CommandParameter);
            }
        }
    }
}
