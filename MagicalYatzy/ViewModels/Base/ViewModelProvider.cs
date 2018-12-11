using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanet.MagicalYatzy.ViewModels.Base
{
    static class ViewModelProvider
    {
        static List<BaseViewModel> viewModels = new List<BaseViewModel>();

        public static T GetViewModel<T>() where T:BaseViewModel
        {
            T vm = (T)viewModels.Where(f=>f is T).FirstOrDefault();
            if (vm == null)
            {
                vm=(T)Activator.CreateInstance(typeof(T));
                viewModels.Add(vm);
            }
            return vm;
        }
        public static T GetNewViewModel<T>() where T : BaseViewModel
        {
            T vm = (T)viewModels.Where(f => f is T).FirstOrDefault();

            if (vm != null)
            {
                viewModels.Remove(vm);
                vm = null;
            }
            vm = (T)Activator.CreateInstance(typeof(T));
            viewModels.Add(vm);
            return vm;
        }

        public static bool HasViewModel<T>() where T : BaseViewModel
        {
            T vm = (T)viewModels.Where(f => f is T).FirstOrDefault();
            return (vm != null);
        }
    }
}
