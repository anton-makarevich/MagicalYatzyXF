using System;
using System.Windows.Input;
using Sanet.MagicalYatzy.Xf.Extensions;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Controls
{
    public class TappableContentView : ContentView
    {
	    private DateTime _startTapTime;

		private readonly int _tapTime = 1500;

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TappableContentView));

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TappableContentView));
        
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public virtual void OnTouchesBegan(Point point)
        {
            if (!IsEnabled) return;
            _startTapTime = DateTime.Now;
            this.AnimateClick();
        }

        // ReSharper disable once UnusedParameter.Global
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
