using System;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.Views
{
    public interface IBaseView
    {
        object ViewModel { get; set; }
    }

    public interface IBaseView<TViewModel> : IBaseView where TViewModel : BaseViewModel
    {
        new TViewModel ViewModel { get; }
    }
}
