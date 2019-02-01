using System.Threading.Tasks;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.Services.Navigation
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
        
        Task<TResult> ShowViewModelForResultAsync<T, TResult>(T viewModel) where T : BaseViewModel where TResult : class;
        Task<TResult> ShowViewModelForResultAsync<T, TResult>() where T : BaseViewModel where TResult : class;

        Task NavigateBackAsync();
        Task CloseAsync();
    }
}