using System.Collections.ObjectModel;
using System.Linq;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class GameViewModel: DicePanelViewModel
    {
        private readonly IGameService _gameService;
        private readonly ISoundsProvider _soundsProvider;
        private ObservableCollection<RollResult> _rollResults;

        public GameViewModel(
            IGameService gameService,
            IDicePanel dicePanel,
            ISoundsProvider soundsProvider):base(dicePanel)
        {
            _gameService = gameService;
            _soundsProvider = soundsProvider;
        }

        public IGame Game => _gameService?.CurrentLocalGame;
        
        public PlayerViewModel CurrentPlayer
        {
            get
            {
                return Game.CurrentPlayer == null ? null : Players.FirstOrDefault(f=>f.Player.InGameId==Game.CurrentPlayer.InGameId);
            }
        }

        public ObservableCollection<PlayerViewModel> Players { get; } = new ObservableCollection<PlayerViewModel>();

        public override void AttachHandlers()
        {
            base.AttachHandlers();
            
            foreach (var player in Game.Players)
            {
                Players.Add(new PlayerViewModel(player));
            }
            
            Game.DiceFixed += GameOnDiceFixed;
            Game.DiceRolled += GameOnDiceRolled;
            Game.PlayerLeft += GameOnPlayerLeft;
            Game.DiceChanged += GameOnDiceChanged;
            Game.PlayerReady += GameOnPlayerReady;
            Game.TurnChanged += GameOnTurnChanged;
        }

        private void GameOnTurnChanged(object sender, MoveEventArgs e)
        {
            DicePanel.UnfixAll();

            if (CurrentPlayer.Player.IsBot)
            {
                Game.ReportRoll();
            }
            NotifyPropertyChanged(nameof(CanRoll));
            RefreshGameStatus();
        }

        private void GameOnPlayerReady(object sender, PlayerEventArgs e)
        {
            var playerViewModel = Players.FirstOrDefault(f => f.Player.InGameId == e.Player.InGameId);
            if (playerViewModel!=null)
                playerViewModel.Player.IsReady = e.Player.IsReady;        
        }

        private void GameOnDiceChanged(object sender, RollEventArgs e)
        {
            _soundsProvider.PlaySound("magic");
            CurrentPlayer.Player.CheckRollResults(new DieResult(){ DiceResults = e.Value.ToList()}, Game.Rules );
            CurrentPlayer.Player.UseArtifact(Artifacts.ManualSet);
            
            if (e.Player.InGameId == CurrentPlayer.Player.InGameId && CurrentPlayer.Player.IsHuman)
            {
                RollResults = new ObservableCollection<RollResult>(CurrentPlayer.Player.Results.Where(f => !f.HasValue && f.ScoreType != Scores.Bonus));
            }
        }

        public ObservableCollection<RollResult> RollResults
        {
            get => _rollResults;
            private set => SetProperty(ref _rollResults, value);
        }

        public bool HasCurrentPlayer => CurrentPlayer != null;
        public bool CanRoll => HasCurrentPlayer 
                               && CurrentPlayer.Player.IsHuman;

        private void GameOnPlayerLeft(object sender, PlayerEventArgs e)
        {
            var playerVm = Players.FirstOrDefault(p => p.Player.InGameId == e.Player.InGameId);
            Players.Remove(playerVm);
        }

        private void GameOnDiceRolled(object sender, RollEventArgs e)
        {
            while (!DicePanel.IsRolling)
            {
                DicePanel.RollDice(e.Value.ToList());
            }

            CurrentPlayer.Player.CheckRollResults(new DieResult(){ DiceResults = e.Value.ToList()},Game.Rules );
        }

        private void GameOnDiceFixed(object sender, FixDiceEventArgs e)
        {
            if (!CurrentPlayer.Player.IsHuman)
                DicePanel.FixDice(e.Value,e.Isfixed);        
        }

        public override void DetachHandlers()
        {
            base.DetachHandlers();
            
            Players.Clear();
            
            Game.DiceFixed -= GameOnDiceFixed;
            Game.DiceRolled -= GameOnDiceRolled;
            Game.PlayerLeft -= GameOnPlayerLeft;
            Game.DiceChanged -= GameOnDiceChanged;
            Game.PlayerReady -= GameOnPlayerReady;
            Game.TurnChanged -= GameOnTurnChanged;

        }

        private void RefreshGameStatus()
        {
            NotifyPropertyChanged(nameof(CurrentPlayer));
            //NotifyPropertyChanged(nameof(RollLabel);
//            NotifyPropertyChanged("CanFix");
//            NotifyPropertyChanged("CanStart");
//
//            if (IsPlayerSelected)
//            {
//                Title = string.Format("{2} {0}, {1}", Game.Move, SelectedPlayer.Name, Messages.GAME_MOVE.Localize());
//                foreach (var pw in Players)
//                    pw.Refresh();
//            }
//            if (Game.Rules.Rule == Rules.krMagic)
//            {
//                NotifyPropertyChanged("IsMagicRollEnabled");
//                NotifyPropertyChanged("IsManualSetEnabled");
//                NotifyPropertyChanged("IsForthRollEnabled");
//                NotifyPropertyChanged("IsMagicRollVisible");
//                NotifyPropertyChanged("IsManualSetVisible");
//                NotifyPropertyChanged("IsForthRollVisible");
//            }
        }
    }
}