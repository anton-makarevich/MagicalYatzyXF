using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
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

        private ObservableCollection<RollResultViewModel> _rollResults;

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
                ? $"{Strings.roll} {Game.Roll}"
                : string.Empty;

        public bool CanFix => HasCurrentPlayer 
                              && CurrentPlayer.Player.IsHuman 
                              && CurrentPlayer.Player.Roll != 1;
        
        public string Title => (HasCurrentPlayer)
            ? $"{Strings.MoveLabel} {Game.Round}, {CurrentPlayer.Player.Name} {Strings.roll} {Game.Roll}"
            : Strings.WaitForPlayersLabel;
 
        public PlayerViewModel CurrentPlayer => 
            Game.CurrentPlayer == null 
                ? null 
                : Players.FirstOrDefault(f=>f.Player.InGameId==Game.CurrentPlayer.InGameId);
        
        public bool IsMagicRollVisible => HasArtifact(Artifacts.MagicalRoll);
        public bool IsRollResetVisible => HasArtifact(Artifacts.RollReset);
        public bool IsManualSetVisible => HasArtifact(Artifacts.ManualSet);

        public string RollImage => "Roll.png";
        public string MagicRollImage => "MagicRoll.png";
        public string ManualSetImage => "ManualSet.png";
        public string RollResetImage => "RollReset.png";

        private bool HasArtifact(Artifacts artifactType)
        {
            if (Game.Rules.CurrentRule != Rules.krMagic)
                return false;
            
            if (!CanRoll)
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
                if ((player.IsHuman || player.IsBot) && !player.IsReady)
                    Game.SetPlayerReady(player,true);
                Players.Add(new PlayerViewModel(player, _localizationService));
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

            DicePanel.RollEnded += DicePanelOnRollEnded;
            DicePanel.DieFixed += DicePanelOnDieFixed;
            
            if (CurrentPlayer != null && CurrentPlayer.Player.IsBot)
            {
                Game.ReportRoll();
            }
            
            RefreshGameStatus();
        }

        private void DicePanelOnDieFixed(object sender, DiceFixedEventArgs e)
        {
            Game.FixDice(e.Value,e.IsFixed);
        }

        private void DicePanelOnRollEnded(object sender, EventArgs e)
        {
            if (!HasCurrentPlayer)
                return;

            if (CurrentPlayer.Player.IsHuman)
            {
                SetRollResults();
            }
            
            //if bot
            if (CurrentPlayer.Player.IsBot)
            {
                if (CurrentPlayer.Player.Roll == 3 || !CurrentPlayer.Player.DecisionMaker.NeedsToRollAgain())
                    CurrentPlayer.Player.DecisionMaker.DecideFill(Game);
                else
                {
                    CurrentPlayer.Player.DecisionMaker.FixDice(Game);
                    if (Game.NumberOfFixedDice == 5)
                        CurrentPlayer.Player.DecisionMaker.DecideFill(Game);
                    else
                        CurrentPlayer.Player.DecisionMaker.DecideRoll(Game, DicePanel);
                }
            }

            RefreshGameStatus();
        }

        private void GameOnResultApplied(object sender, RollResultEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            
            if (e.Value > 0 || e.HasBonus)
            {
                if (e.Value == 50 || e.HasBonus)
                    _soundsProvider.PlaySound("fanfare");
                else
                    _soundsProvider.PlaySound("win");
            }
            else
            {
                _soundsProvider.PlaySound("wrong");
            }

            CurrentPlayer.ApplyRollResult(e);
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
                Players.Add(new PlayerViewModel(e.Player, _localizationService));
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
            CurrentPlayer.Player.CheckRollResults(new DieResult() { DiceResults = e.Value.ToList() }, Game.Rules);
            CurrentPlayer.Player.UseArtifact(Artifacts.ManualSet);
            if (e.Player.InGameId == CurrentPlayer.Player.InGameId && CurrentPlayer.Player.IsHuman)
            {
                SetRollResults();
            }
            RefreshGameStatus();
        }

        private void SetRollResults()
        {
            RollResults = new ObservableCollection<RollResultViewModel>(CurrentPlayer.Results
                .Where(f => !f.HasValue && f.ScoreType != Scores.Bonus));
        }

        private void GameOnPlayerRerolled(object sender, PlayerEventArgs e)
        {
            if (!HasCurrentPlayer)
                return;
            _soundsProvider.PlaySound("magic");
            CurrentPlayer.Player.UseArtifact(Artifacts.RollReset);
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

        public ObservableCollection<RollResultViewModel> RollResults
        {
            get => _rollResults;
            private set => SetProperty(ref _rollResults, value);
        }
        
        public List<string> RollResultsLabels => Game.Rules.ScoresForRule
            .Select(score => new RollResult(score,Game.Rules.CurrentRule))
            .Select(s => _localizationService.GetLocalizedString(s.ScoreType.ToString())).ToList();

        public bool HasCurrentPlayer => CurrentPlayer != null;

        public bool CanRoll => HasCurrentPlayer
                               && CurrentPlayer.Player.IsHuman
                               && !DicePanel.IsRolling
                               && CurrentPlayer.Player.Roll > 0
                               && CurrentPlayer.Player.Roll <= YatzyGame.MaxRoll;

        public string ScoresTitle => Strings.ResultsTableLabel.ToUpper();
        public string PanelTitle => Strings.DiceBoardLabel.ToUpper();
        public ICommand RollCommand => new SimpleCommand(() =>
        {
            if (CanRoll)
            {
                Game?.ReportRoll();
            }
        });

        public ICommand MagicRollCommand => new SimpleCommand(() =>
        {
            if (IsMagicRollVisible)
            {
                Game?.ReportMagicRoll();
            }
        });

        public ICommand ManualSetCommand => new SimpleCommand(() =>
        {
            if (IsManualSetVisible)
            {
                DicePanel.ManualSetMode = true;
            }
        });

        public ICommand RollResetCommand => new SimpleCommand(() =>
        {
            if (IsRollResetVisible)
            {
                Game?.ResetRolls();
            }
        });

        public string MagicRollLabel => GetGameButtonLabel();
        public string ManualSetLabel => GetGameButtonLabel();
        public string RollResetLabel => GetGameButtonLabel();
        public RollResultViewModel SelectedRollResult
        {
            get => null;
            set
            {
                if (value?.RollResult != null)
                {
                    ApplyRollResult(value.RollResult);
                }
            }
        }

        private string GetGameButtonLabel([CallerMemberNameAttribute] string propertyName = "")
        {
            return _localizationService?.GetLocalizedString(propertyName);
        }
        
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

            RollResults = null;
            CurrentPlayer.Player.CheckRollResults(new DieResult(){ DiceResults = e.Value.ToList()},Game.Rules );
            RefreshGameStatus();
        }

        private void GameOnDiceFixed(object sender, FixDiceEventArgs e)
        {
            if (!CurrentPlayer.Player.IsHuman)
                DicePanel.FixDice(e.Value,e.Isfixed);        
        }

        public void ApplyRollResult(IRollResult rollResult)
        {
            Game.ApplyScore(rollResult);
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

            DicePanel.RollEnded -= DicePanelOnRollEnded;
            DicePanel.DieFixed -= DicePanelOnDieFixed;
        }

        private void RefreshGameStatus()
        {
            DicePanel.ClickToFix = CanFix;

            NotifyPropertyChanged(nameof(CurrentPlayer));
            NotifyPropertyChanged(nameof(RollLabel));
            NotifyPropertyChanged(nameof(CanRoll));
            NotifyPropertyChanged(nameof(Title));

            if (HasCurrentPlayer)
            {
                foreach (var pw in Players)
                    pw.Refresh();
            }

            if (Game.Rules.CurrentRule != Rules.krMagic) return;
            NotifyPropertyChanged(nameof(IsMagicRollVisible));
            NotifyPropertyChanged(nameof(IsManualSetVisible));
            NotifyPropertyChanged(nameof(IsRollResetVisible));
        }
    }
}