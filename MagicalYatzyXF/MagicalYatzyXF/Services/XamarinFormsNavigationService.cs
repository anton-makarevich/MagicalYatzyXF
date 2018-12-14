using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Views;
using Sanet.MagicalYatzy.XF.Views.Base;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Services
{
    public class XamarinFormsNavigationService:INavigationService
    {
        private readonly List<BaseViewModel> _viewModels = new List<BaseViewModel>();

        private INavigation FormsNavigation => Application.Current.MainPage.Navigation;

        private readonly Dictionary<Type, Type> _viewModelViewDictionary = new Dictionary<Type, Type>();

        public XamarinFormsNavigationService()
        {
            RegisterViewModels();
        }

        private void RegisterViewModels()
        {
            var assembly = typeof(XamarinFormsNavigationService).GetTypeInfo().Assembly;

            foreach (var type in assembly.DefinedTypes.Where(dt => !dt.IsAbstract &&
                dt.ImplementedInterfaces.Any(ii => ii == typeof(IBaseView))))
            {
                var viewForType = type.ImplementedInterfaces.FirstOrDefault(
                    ii => ii.IsConstructedGenericType &&
                    ii.GetGenericTypeDefinition() == typeof(IBaseView<>));

                _viewModelViewDictionary.Add(viewForType.GenericTypeArguments[0], type.AsType());
            }
        }

        public T GetViewModel<T>() where T : BaseViewModel
        {
            T vm = (T)_viewModels.FirstOrDefault(f => f is T);
            if (vm == null)
            {
                vm = CreateViewModel<T>();
                _viewModels.Add(vm);
            }
            return vm;
        }

        public T GetNewViewModel<T>() where T : BaseViewModel
        {
            T vm = (T)_viewModels.FirstOrDefault(f => f is T);

            if (vm != null)
            {
                _viewModels.Remove(vm);
                vm = null;
            }
            vm = CreateViewModel<T>();
            _viewModels.Add(vm);
            return vm;
        }

        private T CreateViewModel<T>() where T : BaseViewModel
        {
            var vm = App.Container.GetInstance<T>();
            vm.SetNavigationService(this);
            return vm;
        }

        public bool HasViewModel<T>() where T : BaseViewModel
        {
            T vm = (T)_viewModels.FirstOrDefault(f => f is T);
            return (vm != null);
        }

        public Task NavigateToViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            return OpenViewModelAsync(viewModel, false);
        }

        public Task NavigateToViewModelAsync<T>() where T : BaseViewModel
        {
            var vm = GetViewModel<T>();
            return OpenViewModelAsync(vm, false);
        }

        private async Task OpenViewModelAsync<T>(T viewModel, bool modalPresentation = false) where T : BaseViewModel
        {
            if (viewModel == null)
                return;
            if (viewModel.NavigationService == null)
                viewModel.SetNavigationService(this);
            if (CreateView(viewModel) is BaseView<T> view)
            {
                if (modalPresentation)
                    await FormsNavigation.PushModalAsync(view);
                else
                    await FormsNavigation.PushAsync(view);
            }
        }

        private IBaseView CreateView(BaseViewModel viewModel)
        {
            var viewModelType = viewModel.GetType();

            var viewType = _viewModelViewDictionary[viewModelType];

            var view = (IBaseView)Activator.CreateInstance(viewType);

            view.ViewModel = viewModel;

            return view;
        }

        public async Task NavigateBack()
        {
            await FormsNavigation.PopAsync();
        }

        public async Task CloseAsync()
        {
            await FormsNavigation.PopModalAsync();
        }

        public Task ShowViewModelAsync<T>(T viewModel) where T : BaseViewModel
        {
            return OpenViewModelAsync(viewModel, true);
        }

        public Task ShowViewModelAsync<T>() where T : BaseViewModel
        {
            var viewModel = GetViewModel<T>();
            return OpenViewModelAsync(viewModel, true);
        }
    }
}
