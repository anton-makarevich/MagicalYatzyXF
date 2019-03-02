using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Services.Navigation;

namespace Sanet.MagicalYatzy.ViewModels.Base
{
    public abstract class BaseViewModel : BindableBase
    {
        public event EventHandler<object> OnResult; 
        
        #region Fields
        private bool _isBusy;
        private int _pageWidth;
        private INavigationService _navigationService;
        #endregion
        protected BaseViewModel() { }

        protected BaseViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        #region Properties

        // ReSharper disable once LocalizableElement
        public INavigationService NavigationService => _navigationService ?? throw new ArgumentNullException(
                                                           nameof(NavigationService), 
                                                           "Navigation service should be initialized, check your App.cs");

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public int PageWidth
        {
            get => _pageWidth;
            set => SetProperty(ref _pageWidth, value);
        }
        
        public bool ExpectsResult { get; set; }
        #endregion

        #region Commands
        public ICommand BackCommand => new SimpleCommand(async () => await GoBackAsync());
        #endregion

        #region Methods
        public void SetNavigationService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async Task GoBackAsync()
        {
            await NavigationService.NavigateBackAsync();
        }
        
        internal async Task CloseAsync(object result = null)
        {
            await NavigationService.CloseAsync();
            if (ExpectsResult)
            {
                ExpectsResult = false;
                OnResult?.Invoke(this, result);
            }
        }

        public virtual void AttachHandlers() { }

        public virtual void DetachHandlers() { }

        #endregion
    }
}
