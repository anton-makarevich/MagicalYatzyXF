using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class PlayerViewModel:BindableBase
    {
        private readonly IPlayer _player;
        private bool _canBeDeleted = true;
        private List<RollResultViewModel> _results;

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

        public void Refresh()
        {
            NotifyPropertyChanged(nameof(Results));
        }
        
        public int Total => _player.Total;

        public List<RollResultViewModel> Results =>
            Player.Results == null
                ? null
                : _results ?? (_results = Player.Results.Select(r => new RollResultViewModel(r)).ToList());

        public void ApplyRollResult(IRollResult result)
        {
            var rollResult = Player.Results.FirstOrDefault(f => f.ScoreType == result.ScoreType);
            if (rollResult != null)
            {
                rollResult.Value = result.PossibleValue;
                rollResult.HasBonus = result.HasBonus;
            }

            NotifyPropertyChanged(nameof(Total));
        }
    }
}