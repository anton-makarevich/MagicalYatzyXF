using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class PlayerViewModel:BindableBase
    {
        private readonly IPlayer _player;

        public PlayerViewModel(IPlayer player)
        {
            _player = player;
        }

        public string Name => _player.Name;

        public string Image => _player.ProfileImage;
        public string DeleteImage => "close.png";
    }
}