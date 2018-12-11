using System.Threading.Tasks;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.Services
{
    public interface INavigationService
    {
        T GetNewViewModel<T>() where T : BaseViewModel;
        T GetViewModel<T>() where T : BaseViewModel;
        bool HasViewModel<T>() where T : BaseViewModel;

        Task NavigateToViewModelAsync<T>(T viewModel) where T : BaseViewModel;
        Task NavigateToViewModelAsync<T>() where T : BaseViewModel;

        Task ShowViewModelAsync<T>(T viewModel) where T : BaseViewModel;
        Task ShowViewModelAsync<T>() where T : BaseViewModel;

        Task NavigateBack();
        Task CloseAsync();
    }
}