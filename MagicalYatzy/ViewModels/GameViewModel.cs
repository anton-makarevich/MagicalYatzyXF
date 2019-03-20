using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels
{
    public class GameViewModel : DicePanelViewModel
    {
        private readonly IGameService _gameService;
        private readonly ISoundsProvider _soundsProvider;
        private readonly ILocalizationService _localizationService;

        private ObservableCollection<RollResult> _rollResults;

        public GameViewModel(
            IGameService gameService,
            IDicePanel dicePanel,
            ISoundsProvider soundsProvider,
            ILocalizationService localizationService) : base(dicePanel)
        {
            _gameService = gameService;
            _soundsProvider = soundsProvider;
            _localizationService = localizationService;
        }

        public IGame Game => _gameService?.CurrentLocalGame;

        public string RollLabel =>
            CurrentPlayer != null
                ? $"{Strings.roll} {CurrentPlayer.Player.Roll}"
                : string.Empty;

        public bool CanFix => HasCurrentPlayer 
                              && CurrentPlayer.Player.IsHuman 
                              && CurrentPlayer.Player.Roll != 1;
        
        public string Title => (HasCurrentPlayer)
            ? $"{Strings.roll} {Game.Round}, {CurrentPlayer.Player.Name}"
            : Strings.WaitForPlayersLabel;
 
        public PlayerViewModel CurrentPlayer => 
            Game.CurrentPlayer == null 
                ? null 
                : Players.FirstOrDefault(f=>f.Player.InGameId==Game.CurrentPlayer.InGameId);
        
        public bool IsMagicRollVisible => HasArtifact(Artifacts.MagicalRoll);
        public bool IsFourthRollVisible => HasArtifact(Artifacts.FourthRoll);
        public bool IsManualSetVisible => HasArtifact(Artifacts.ManualSet);

        public string RollImage => "Roll.png";
        public string MagicRollImage => "MagicRoll.png";
        public string ManualSetImage => "ManualSet.png";
        public string RollResetImage => "RollReset.png";

        private bool HasArtifact(Artifacts artifactType)
        {
            if (Game.Rules.CurrentRule != Rules.krMagic)
                return false;
            
            if (!HasCurrentPlayer)
                return false;
            
            var artifact = CurrentPlayer.Player.MagicalArtifactsForGame
                .FirstOrDefault(a=>a.Type == artifactType);
 
            return artifact !=null
                   && !artifact.IsUsed;
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
            Game.GameFinished += GameOnGameFinished;
            Game.PlayerJoined += GameOnPlayerJoined;
            Game.StyleChanged += GameOnStyleChanged;
            Game.ResultApplied += GameOnResultApplied;
            Game.PlayerRerolled += GameOnPlayerRerolled;
            Game.MagicRollUsed += GameOnMagicRollUsed;
        }

        private void GameOnResultApplied(object sender, ResultEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            
            if (e.Result.PossibleValue > 0 || e.Result.HasBonus)
            {
                if (e.Result.PossibleValue == 50 || e.Result.HasBonus)
                    _soundsProvider.PlaySound("fanfare");
                else
                    _soundsProvider.PlaySound("win");
            }
            else
            {
                _soundsProvider.PlaySound("wrong");
            }

            CurrentPlayer.ApplyRollResult(e.Result);
            RefreshGameStatus();
            RollResults = null;
        }

        private void GameOnStyleChanged(object sender, PlayerEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            CurrentPlayer.Player.SelectedStyle = e.Player.SelectedStyle;
        }

        private void GameOnPlayerJoined(object sender, PlayerEventArgs e)
        {
            if (e?.Player != null)
                Players.Add(new PlayerViewModel(e.Player));
        }

        private async void GameOnGameFinished(object sender, EventArgs e)
        {
            await NavigationService.NavigateToViewModelAsync<GameResultsViewModel>();
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
            if (!HasCurrentPlayer)
                return;
            _soundsProvider.PlaySound("magic");
            CurrentPlayer.Player.CheckRollResults(new DieResult(){ DiceResults = e.Value.ToList()}, Game.Rules );
            CurrentPlayer.Player.UseArtifact(Artifacts.ManualSet);
            
            if (e.Player.InGameId == CurrentPlayer.Player.InGameId && CurrentPlayer.Player.IsHuman)
            {
                RollResults = new ObservableCollection<RollResult>(CurrentPlayer.Player.Results.Where(f => !f.HasValue && f.ScoreType != Scores.Bonus));
            }
            RefreshGameStatus();
        }
        
        private void GameOnPlayerRerolled(object sender, PlayerEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            _soundsProvider.PlaySound("magic");
            CurrentPlayer.Player.Roll = 1;
            CurrentPlayer.Player.UseArtifact(Artifacts.FourthRoll);
            RollResults = null;
            RefreshGameStatus();            
        }
        
        
        private void GameOnMagicRollUsed(object sender, PlayerEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            _soundsProvider.PlaySound("magic");
            CurrentPlayer.Player.UseArtifact(Artifacts.MagicalRoll);
            RefreshGameStatus();
        }


        public ObservableCollection<RollResult> RollResults
        {
            get => _rollResults;
            private set => SetProperty(ref _rollResults, value);
        }
        
        public List<string> RollResultsLabels => Game.Rules.ScoresForRule
            .Select(score => new RollResult(score))
            .Select(s => _localizationService.GetLocalizedString(s.ScoreType.ToString())).ToList();

        public bool HasCurrentPlayer => CurrentPlayer != null;
        public bool CanRoll => HasCurrentPlayer 
                               && CurrentPlayer.Player.IsHuman;

        public string ScoresTitle => Strings.ResultsTableLabel.ToUpper();
        public string PanelTitle => Strings.DiceBoardLabel.ToUpper();
        public ICommand RollCommand => new SimpleCommand(() =>
        {
            if (CanRoll)
            {
                Game?.ReportRoll();
            }
        });

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
            Game.GameFinished -= GameOnGameFinished;
            Game.PlayerJoined -= GameOnPlayerJoined;
            Game.StyleChanged -= GameOnStyleChanged;
            Game.ResultApplied -= GameOnResultApplied;            
            Game.PlayerRerolled -= GameOnPlayerRerolled;
            Game.MagicRollUsed -= GameOnMagicRollUsed;
        }

        private void RefreshGameStatus()
        {
            NotifyPropertyChanged(nameof(CurrentPlayer));
            NotifyPropertyChanged(nameof(RollLabel));
            NotifyPropertyChanged(nameof(CanFix));
            NotifyPropertyChanged(nameof(Title));

            if (HasCurrentPlayer)
            {
                foreach (var pw in Players)
                    pw.Refresh();
            }

            if (Game.Rules.CurrentRule != Rules.krMagic) return;
            NotifyPropertyChanged(nameof(IsMagicRollVisible));
            NotifyPropertyChanged(nameof(IsManualSetVisible));
            NotifyPropertyChanged(nameof(IsFourthRollVisible));
        }
    }
}