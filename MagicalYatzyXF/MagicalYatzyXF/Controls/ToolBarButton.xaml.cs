using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Controls
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
        
        public object ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        private static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ToolBarButton)bindable;

            var incoming = newValue as ImageSource;
            control.Icon.Source = incoming;
        }
    }
}