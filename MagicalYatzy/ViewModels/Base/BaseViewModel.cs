using Sanet.MagicalYatzy.Services;
using System;

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

        public INavigationService NavigationService => _navigationService ?? throw new ArgumentNullException("Navigation service should be initialized, check your App.cs");

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

        #region Methods
        public void SetNavigationService(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public virtual void AttachHandlers()
        {
            
        }

        public virtual void DetachHandlers()
        {
            
        }
#endregion
    }
}
