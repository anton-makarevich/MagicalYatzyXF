using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls
{
    public partial class ToolBarButton
    {
        public ToolBarButton()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource), 
            typeof(ToolBarButton),
            default(ImageSource), 
            propertyChanged: OnImageSourceChanged);
        
        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        private static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ToolBarButton)bindable;

            var incoming = newValue as ImageSource;
            control.Icon.Source = incoming;
        }
        
        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text),
            typeof(string), 
            typeof(ToolBarButton),
            default(string), 
            propertyChanged: OnTextChanged);
        
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ToolBarButton)bindable;

            var incoming = newValue as string;
            control.Label.Text = incoming;
        }
    }
}