using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Controls.TabControl
{
    public class TabBarButton : TappableContentView
    {
        private readonly Label _label;

        public Color DarkTextColor = Color.Black;
        public Color AccentColor = Color.Accent;

        public TabBarButton()
        {
            _label = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 14,
                LineBreakMode = LineBreakMode.TailTruncation,
                TextColor = DarkTextColor,
            };

            Content = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                Padding = 10,
            };

            (Content as StackLayout).Children.Add(_label);
        }

        public TabBarButton(string buttonText) : this()
        {
            _label.Text = buttonText;
        }

        public static BindableProperty ButtonTextProperty =
            BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(TabBarButton), null,
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabBarButton)bindable;
                    ctrl.ButtonText = (string)newValue;
                });
                
        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set
            {
                SetValue(ButtonTextProperty, value);
                _label.Text = value;
            }
        }

        public static BindableProperty SelectedColorProperty =
            BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(TabBarButton), Color.Accent,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabBarButton)bindable;
                    ctrl.SelectedColor = (Color)newValue;
                });
                
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set
            {
                SetValue(SelectedColorProperty, value);
                AccentColor = value;
            }
        }

        public static BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(TabBarButton), 14.0,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabBarButton)bindable;
                    ctrl.FontSize = (double)newValue;
                });
                
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set
            {
                SetValue(FontSizeProperty, value);
                _label.FontSize = value;
            }
        }

        public static BindableProperty FontFamilyProperty =
            BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(TabBarButton), null,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabBarButton)bindable;
                    ctrl.FontFamily = (string)newValue;
                });
                
        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set
            {
                SetValue(FontFamilyProperty, value);
                _label.FontFamily = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _label.TextColor == AccentColor;
            }
            set
            {
                if (value)
                    _label.TextColor = AccentColor;
                else
                    _label.TextColor = DarkTextColor;
            }
        }

        public override void OnTouchesBegan(Point position)
        {
            _label.TextColor = AccentColor;
        }
    }
}
