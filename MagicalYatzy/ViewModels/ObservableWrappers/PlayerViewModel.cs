using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers
{
    public class PlayerViewModel:BindableBase
    {
        private readonly IPlayer _player;
        private readonly ILocalizationService _localizationService;
        private bool _canBeDeleted = true;
        private List<RollResultViewModel> _results;

        public event EventHandler PlayerDeleted;

        public PlayerViewModel(IPlayer player, ILocalizationService localizationService)
        {
            _player = player;
            _localizationService = localizationService;
        }

        public string Name => _player.Name;
        
        public string TypeName => _player.Type switch
        {
            PlayerType.Local or PlayerType.Network => _localizationService.GetLocalizedString("PlayerNameDefault"),
            PlayerType.AI => _localizationService.GetLocalizedString("BotNameDefault"),
            _ => throw new ArgumentOutOfRangeException()
        };

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
            NotifyPropertyChanged(nameof(IsMyTurn));
        }
        
        public int Total => _player.Total;

        public List<RollResultViewModel> Results =>
            Player.Results == null
                ? null
                : _results ?? (_results = Player.Results.Select(r => new RollResultViewModel(r,_localizationService)).ToList());

        public bool IsMyTurn => _player.IsMyTurn;

        public void ApplyRollResult(RollResultEventArgs result)
        {
            var rollResult = Results.FirstOrDefault(f => f.ScoreType == result.ScoreType);
            rollResult?.ApplyResult((result.Value,result.HasBonus));

            NotifyPropertyChanged(nameof(Total));
        }
    }
}