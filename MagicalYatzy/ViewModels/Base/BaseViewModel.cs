using Sanet.MagicalYatzy.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models.Events;

namespace Sanet.MagicalYatzy.ViewModels.Base
{
    public abstract class BaseViewModel : BindableBase
    {
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

        public INavigationService NavigationService => _navigationService ?? throw new ArgumentNullException("NavigationService", "Navigation service should be initialized, check your App.cs");

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }

        public int PageWidth
        {
            get { return _pageWidth; }
            set
            {
                SetProperty(ref _pageWidth, value);
            }
        }
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

        public virtual void AttachHandlers() { }

        public virtual void DetachHandlers() { }

        #endregion
    }
}
