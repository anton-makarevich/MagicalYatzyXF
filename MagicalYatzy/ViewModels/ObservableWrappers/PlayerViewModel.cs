using System;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class PlayerViewModel:BindableBase
    {
        private readonly IPlayer _player;
        private bool _canBeDeleted = true;

        public event EventHandler PlayerDeleted;

        public PlayerViewModel(IPlayer player)
        {
            _player = player;
        }

        public string Name => _player.Name;

        public string Image => _player.ProfileImage;
        public string DeleteImage => "close.png";
        public ICommand DeleteCommand => new SimpleCommand(() =>
        {
            if (CanBeDeleted)
                PlayerDeleted?.Invoke(this, null);
        });

        public bool CanBeDeleted
        {
            get => _canBeDeleted;
            set => SetProperty(ref _canBeDeleted, value);
        }

        public IPlayer Player => _player;
    }
}