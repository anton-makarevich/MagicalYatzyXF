using Sanet.MagicalYatzy.ViewModels.Base;
using Xamarin.Forms;
using Sanet.MagicalYatzy.XF.Controls;
using Sanet.MagicalYatzy.XF.Helpers;

namespace Sanet.MagicalYatzy.XF.Views.Base
{
    public class NavigationBackView<TViewModel> : DicePanelView<TViewModel> where TViewModel : DicePanelViewModel
    {
        private const int BackButtonSize = 50;

        private TappableContentView _backButton;
        protected void InitBackButton()
        { 
            if (_backButton != null)
                return;
            if (Content is Grid pageGrid)
            {
                var backImage = new Image
                {
                    Source = ImageSource.FromFile(ImageHelper.Get("backarrow.png")),
                    Aspect = Aspect.AspectFit,
                    InputTransparent = Device.RuntimePlatform == Device.macOS
                };
                _backButton = new TappableContentView()
                {
                    WidthRequest = BackButtonSize,
                    HeightRequest = BackButtonSize,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(20, 10),
                    Content = backImage,
                    Command = ViewModel.BackCommand
                };

                pageGrid.Children.Add(_backButton);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InitBackButton();
        }
    }
}

