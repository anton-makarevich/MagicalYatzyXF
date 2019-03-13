using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class RollResultViewModel:BindableBase
    {
        private readonly RollResult _rollResult;

        public RollResultViewModel(RollResult rollResult)
        {
            _rollResult = rollResult;
        }

        public RollResult RollResult => _rollResult;
    }
}