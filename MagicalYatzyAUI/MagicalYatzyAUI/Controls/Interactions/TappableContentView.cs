using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Sanet.MagicalYatzy.Avalonia.Extensions;

namespace Sanet.MagicalYatzy.Avalonia.Controls.Interactions
{
    public class TappableContentView : ContentControl
    {
        private DateTime _startTapTime;
        private readonly int _tapTime = 1500;

        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<TappableContentView, ICommand>(nameof(Command));

        public ICommand Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<TappableContentView, object>(nameof(CommandParameter));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public TappableContentView()
        {
            AddHandler(PointerPressedEvent, OnPointerPressed);
            AddHandler(PointerReleasedEvent, OnPointerReleased);
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!IsEnabled)
                return;

            _startTapTime = DateTime.Now;
            this.AnimateClick();
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
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