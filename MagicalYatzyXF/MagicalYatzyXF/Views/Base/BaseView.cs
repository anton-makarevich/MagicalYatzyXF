using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Views;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Sanet.MagicalYatzy.XF.Views.Base
{
    public abstract class BaseView<TViewModel> : ContentPage, IBaseView<TViewModel> where TViewModel : BaseViewModel
    {
        protected bool NavigationBarEnabled = false;

        private TViewModel _viewModel;

        protected BaseView() : base()
        {
            BackgroundColor = Color.Black;

            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);
        }

        public virtual TViewModel ViewModel
        {
            get => _viewModel; 
            set
            {
                _viewModel = value;
                // TODO: adjust according to device
                var pageWidth = 300;
                _viewModel.PageWidth = pageWidth;
                BindingContext = _viewModel;
            }
        }

        object IBaseView.ViewModel
        {
            get { return _viewModel; }
            set
            {
                ViewModel = (TViewModel)value;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ViewModel.AttachHandlers();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.DetachHandlers();
        }
    }
}
