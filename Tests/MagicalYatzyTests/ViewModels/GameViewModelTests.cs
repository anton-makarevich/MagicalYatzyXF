using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Ai;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.Services.Media;
using Sanet.MagicalYatzy.Utils;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MagicalYatzyTests.ViewModels;

public class GameViewModelTests
{
    private readonly GameViewModel _sut;
    private readonly IGameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly IDicePanel _dicePanel;
    private readonly ISoundsProvider _soundsProvider;
    private readonly ILocalizationService _localizationService;
    private readonly IPlayer _humanPlayer;
    private readonly IPlayer _botPlayer;
    private readonly IRollResult _rollResult;

    public GameViewModelTests()
    {
        _humanPlayer = Substitute.For<IPlayer>();
        _humanPlayer.InGameId.Returns("0");
        _humanPlayer.Roll = 1;

        _botPlayer = Substitute.For<IPlayer>();
        _botPlayer.InGameId.Returns("1");

        _rollResult = Substitute.For<IRollResult>();

        _gameService = Substitute.For<IGameService>();
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krSimple));
        _gameService.CurrentLocalGame.Players.Returns(new List<IPlayer>()
        {
            _humanPlayer,
            _botPlayer
        });

        _navigationService = Substitute.For<INavigationService>();

        
        _localizationService = Substitute.For<ILocalizationService>();

        _dicePanel = Substitute.For<IDicePanel>();
        _soundsProvider = Substitute.For<ISoundsProvider>();
        _sut = new GameViewModel(_gameService, _dicePanel, _soundsProvider, _localizationService);
    }

    [Fact]
    public void HasGame()
    {
        Assert.NotNull(_sut.Game);
    }

    [Fact]
    public void DoesNotHavePlayersFromGameServiceWhenUntilViewIsNotActive()
    {
        Assert.Empty(_sut.Players);
    }

    [Fact]
    public void ClearPlayersOnViewHiding()
    {
        _sut.AttachHandlers();
        Assert.NotEmpty(_sut.Players);
        _sut.DetachHandlers();
        Assert.Empty(_sut.Players);
    }
        
    [Fact]
    public void HumanAndBotPlayersGetReadyAutomaticallyForLocalGame()
    {
        _humanPlayer.IsHuman.Returns(true);
        _botPlayer.IsBot.Returns(true);
            
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.Received().SetPlayerReady(_humanPlayer,true);
        _gameService.CurrentLocalGame.Received().SetPlayerReady(_botPlayer,true);
    }

    [Fact]
    public void RefreshesGameStatusOnPageActivation()
    {
        CheckIfGameStatusHasBeenRefreshed(()=>{_sut.AttachHandlers();});
    }

    [Fact]
    public void HasPlayersFromGameServiceWhenViewIsActive()
    {
        _sut.AttachHandlers();
        Assert.NotEmpty(_sut.Players);
        Assert.Equal(_gameService.CurrentLocalGame.Players.Count, _sut.Players.Count);
    }

    [Fact]
    public void HasCurrentPlayer()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        Assert.NotNull(_sut.CurrentPlayer);
    }

    [Fact]
    public void HasCurrentPlayerIsTrueWhenThereIsCurrentPlayer()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        Assert.True(_sut.HasCurrentPlayer);
    }

    [Fact]
    public void HasCurrentPlayerIsFalseWhenThereIsNoCurrentPlayer()
    {
        Assert.False(_sut.HasCurrentPlayer);
    }

    [Fact]
    public void GameOnDiceFixedFixesDicePanelDiceIfCurrentPlayerIsNotHuman()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceFixed +=
            Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer, 2, true));

        _dicePanel.Received().FixDice(2, true);
    }

    [Fact]
    public void GameOnDiceFixedDoesNotFixDicePanelDiceWhenViewIsNotActive()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();

        _gameService.CurrentLocalGame.DiceFixed +=
            Raise.EventWith(null, new FixDiceEventArgs(_humanPlayer, 2, true));

        _dicePanel.DidNotReceive().FixDice(2, true);
    }

    [Fact]
    public void GameOnDiceRolledCallsRollDiceOnDicePanel()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _dicePanel.IsRolling.Returns(false, true);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceRolled +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        _dicePanel.Received(1).RollDice(Arg.Any<List<int>>());
    }

    [Fact]
    public void GameOnDiceRolledDoesNotCallRollDiceOnDicePanelWhenViewIsNotActive()
    {
        _dicePanel.IsRolling.Returns(false, true);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();
        _sut.DetachHandlers();
        _gameService.CurrentLocalGame.DiceRolled +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        _dicePanel.DidNotReceive().RollDice(Arg.Any<List<int>>());
    }

    [Fact]
    public void GameOnDiceRollCallsCheckRollResultsForCurrentPlayer()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _dicePanel.IsRolling.Returns(true);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceRolled +=
            Raise.EventWith(null, new RollEventArgs(_botPlayer, results));

        _humanPlayer.ReceivedWithAnyArgs().CheckRollResults(null, null);
    }
        
    [Fact]
    public void GameOnDiceRolledRefreshesGameStatus()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _dicePanel.IsRolling.Returns(true);
        var testAction = new Action(() => { 
            var results = new[] {2, 4, 6, 2, 1};
            if (!_sut.Players.Any())
                _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceRolled +=
                Raise.EventWith(null, new RollEventArgs(_botPlayer, results));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }

    [Fact]
    public void GameOnDiceRolledRemovesCurrentRollResults()
    {
        _dicePanel.IsRolling.Returns(true);
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] { 2, 4, 6, 2, 1 };
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        Assert.NotNull(_sut.RollResults);

        _gameService.CurrentLocalGame.DiceRolled +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        Assert.Null(_sut.RollResults);
    }

    [Fact]
    public void GameOnPlayerLeftRemovesPlayer()
    {
        _sut.AttachHandlers();
        var initialPlayersCount = _sut.Players.Count;

        _gameService.CurrentLocalGame.PlayerLeft +=
            Raise.EventWith(null, new PlayerEventArgs(_botPlayer));

        Assert.Equal(initialPlayersCount - 1, _sut.Players.Count);
        Assert.Null(_sut.Players.FirstOrDefault(p => p.Player.InGameId == _botPlayer.InGameId));
    }

    [Fact]
    public void GameOnDiceChangedPlaysMagicSound()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        _soundsProvider.Received().PlaySound("magic");
    }

    [Fact]
    public void GameOnDiceChangedUpdatesCurrentPlayersResults()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        _humanPlayer.ReceivedWithAnyArgs().CheckRollResults(null, null);
    }

    [Fact]
    public void GameOnDiceChangedConsumesPlayersArtifact()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        _humanPlayer.Received().UseArtifact(Artifacts.ManualSet);
    }

    [Fact]
    public void GameOnDiceChangedShowsResultsToSelectIfPlayerIsHuman()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        Assert.NotNull(_sut.RollResults);
    }
        
    [Fact]
    public void GameOnDiceChangedRefreshesGameStatus()
    {
        var testAction = new Action(() =>
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            var results = new[] {2, 4, 6, 2, 1};
            if (!_sut.Players.Any())
                _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }
        
    [Fact]
    public void GameOnDiceChangedDoesNotRefreshGameStatusIfThereIsNoCurrentPlayer()
    {
        var testAction = new Action(() =>
        {
            var results = new[] {2, 4, 6, 2, 1};
            if (!_sut.Players.Any())
                _sut.AttachHandlers();

            _gameService.CurrentLocalGame.DiceChanged +=
                Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction,0);
    }

    [Fact]
    public void GameOnPlayerReadyUpdatePlayerIsReadyStatus()
    {
        var remoteHumanPlayer = Substitute.For<IPlayer>();
        remoteHumanPlayer.InGameId.Returns("0");
        remoteHumanPlayer.IsReady.Returns(true);
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.PlayerReady +=
            Raise.EventWith(null, new PlayerEventArgs(remoteHumanPlayer));

        Assert.True(_humanPlayer.IsReady);
    }

    [Fact]
    public void CanRollIsFalseWhenThereIsNoCurrentPlayer()
    {
        Assert.False(_sut.CanRoll);
    }

    [Fact]
    public void CanRollIsFalseWhenPlayersRollIsOutsideOfRange()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.Roll = YatzyGame.MaxRoll+1;
        _sut.AttachHandlers();
            
        Assert.False(_sut.CanRoll);
    }
        
    [Fact]
    public void CanRollIsTrueWhenPlayersRollIsEqualToMaxRolls()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.Roll = YatzyGame.MaxRoll;
        _sut.AttachHandlers();
            
        Assert.True(_sut.CanRoll);
    }
        
    [Fact]
    public void CanRollIsTrueWhenCurrentPlayerIsHuman()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        Assert.True(_sut.CanRoll);
    }
        
    [Fact]
    public void CanRollIsFalseWhenCurrentPlayerIsNotHuman()
    {
        _botPlayer.IsHuman.Returns(false);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();

        Assert.False(_sut.CanRoll);
    }

    [Fact]
    public void CanRollIsFalseWhenDiceAreRolling()
    {
        _botPlayer.IsHuman.Returns(true);
        _dicePanel.IsRolling.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();

        Assert.False(_sut.CanRoll);
    }

    [Fact]
    public void GameOnTurnChangedCallsUnfixAllOnDicePanel()
    {
        // Arrange
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        // Act
        _gameService.CurrentLocalGame.TurnChanged +=
            Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
            
        // Assert
        _dicePanel.Received().UnfixAll();
    }
        
    [Fact]
    public void GameOnTurnChangedForceBotPlayerToMakeARoll()
    {
        // Arrange
        _botPlayer.IsBot.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
            
        // Act
        _gameService.CurrentLocalGame.TurnChanged +=
            Raise.EventWith(null, new MoveEventArgs(_botPlayer,1));
            
        // Assert
        _gameService.CurrentLocalGame.Received().ReportRoll();
    }
        
    [Fact]
    public void GameOnTurnChangedRefreshesGameStatus()
    {
        var testAction = new Action(() =>
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
            if (_sut.Players.Count == 0)
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.TurnChanged +=
                Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }
        
    [Fact]
    public void GameOnTurnChangedDoesNotCallUnfixAllOnDicePanelIfViewIsNotActive()
    {
        // Arrange
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        // Act
        _gameService.CurrentLocalGame.TurnChanged +=
            Raise.EventWith(null, new MoveEventArgs(_humanPlayer,1));
            
        // Assert
        _dicePanel.DidNotReceive().UnfixAll();
    }

    [Fact]
    public void RollLabelIsEmptyIfThereIsNoCurrentPlayer()
    {
        Assert.Empty(_sut.RollLabel);
    }
        
    [Fact]
    public void RollLabelContainsCurrentPlayerRoll()
    {
        _botPlayer.Roll.Returns(123);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            
        Assert.Contains(_sut.RollLabel, _botPlayer.Roll.ToString());
    }

    [Fact]
    public void CanFixIfThereIsCurrentHumanPlayerAndItIsNotFirstRoll()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll.Returns(2);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.True(_sut.CanFix);
    }
        
    [Fact]
    public void CanNotFixIfThereIsCurrentHumanPlayerAndItIsFirstRoll()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll.Returns(1);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.False(_sut.CanFix);
    }
        
    [Fact]
    public void CanNotFixIfCurrentPlayerIsNotHuman()
    {
        _botPlayer.IsHuman.Returns(false);
        _botPlayer.Roll.Returns(2);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
            
        Assert.False(_sut.CanFix);
    }
        
    [Fact]
    public void CanNotFixIfThereIsNoCurrentPlayer()
    {
        _sut.AttachHandlers();
            
        Assert.False(_sut.CanFix);
    }

    [Fact]
    public void TitleContainsCurrentRoundIfThereIsCurrentPlayer()
    {
        _gameService.CurrentLocalGame.Round.Returns(2);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.Contains(_gameService.CurrentLocalGame.Round.ToString(),_sut.Title);
    }
        
    [Fact]
    public void TitleContainsCurrentPlayersName()
    {
        _humanPlayer.Name.Returns("Player 1");
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.Contains(_humanPlayer.Name,_sut.Title);
    }
        
    [Fact]
    public void TitleIs_WaitingForPlayers_When_ThereIsNoPlayer()
    {
        _sut.AttachHandlers();
        
        _localizationService.GetLocalizedString("WaitForPlayersLabel").Returns(Strings.WaitForPlayersLabel);
            
        Assert.Contains(Strings.WaitForPlayersLabel,_sut.Title);
    }
    
    [Fact]
    public void TitleContainsMoveLabel_WhenThereIsAPlayer()
    {
        _sut.AttachHandlers();
        
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _localizationService.GetLocalizedString("MoveLabel").Returns(Strings.MoveLabel);
            
        _sut.Title.Should().Contain(Strings.MoveLabel);
    }

    [Fact]
    public void MagicRollIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.MagicalRoll)});
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.True(_sut.IsMagicRollVisible);
    }
        
    [Fact]
    public void MagicRollIsInvisibleWhenCurrentPlayerHasCorrespondingArtifactButRulesAreNotMagical()
    {
        _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.MagicalRoll)});
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krSimple));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.False(_sut.IsMagicRollVisible);
    }
        
    [Fact]
    public void MagicRollIsInvisibleWhenCurrentPlayerHasCorrespondingArtifactButItsUsed()
    {
        var usedArtifact = new Artifact(Artifacts.MagicalRoll);
        usedArtifact.Use();
        _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {usedArtifact});
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.False(_sut.IsMagicRollVisible);
    }
        
    [Fact]
    public void FourthRollIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.RollReset)});
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.True(_sut.IsRollResetVisible);
    }

    [Fact]
    public void ManualSetIsVisibleWhenCurrentPlayerHasCorrespondingArtifact()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new List<Artifact>() {new Artifact(Artifacts.ManualSet)});
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        Assert.True(_sut.IsManualSetVisible);
    }

    [Fact]
    public void GameOnFinishedRedirectsToGameResultsScreen()
    {
        _sut.SetNavigationService(_navigationService);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.GameFinished +=
            Raise.EventWith(null, new EventArgs());

        _navigationService.Received().NavigateToViewModelAsync<GameResultsViewModel>();
    }
        
    [Fact]
    public void GameOnFinishedDoesNotRedirectToGameResultsScreenWhenViewIsNotActive()
    {
        _sut.SetNavigationService(_navigationService);
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.GameFinished +=
            Raise.EventWith(null, new EventArgs());

        _navigationService.DidNotReceive().NavigateToViewModelAsync<GameResultsViewModel>();
    }

    [Fact]
    public void GameOnPlayerJoinedAddsNewPlayerViewModelToCollection()
    {
        _sut.AttachHandlers();
        var initialPlayersAmount = _sut.Players.Count;
            
        var newPlayer = new Player(PlayerType.Local);
            
        _gameService.CurrentLocalGame.PlayerJoined +=
            Raise.EventWith(null, new PlayerEventArgs(newPlayer));
            
        Assert.Equal(initialPlayersAmount+1,_sut.Players.Count);
    }
        
    [Fact]
    public void GameOnPlayerJoinedHandledNullArgument()
    {
        _sut.AttachHandlers();
        var initialPlayersAmount = _sut.Players.Count;
            
        _gameService.CurrentLocalGame.PlayerJoined +=
            Raise.EventWith(null, new PlayerEventArgs(null));
            
        Assert.Equal(initialPlayersAmount,_sut.Players.Count);
    }
        
    [Fact]
    public void GameOnPlayerJoinedDoesNotAddNewPlayerViewModelToCollectionIfViewIsNotActive()
    {
        _sut.AttachHandlers();
            
        var newPlayer = new Player(PlayerType.Local);
            
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.PlayerJoined +=
            Raise.EventWith(null, new PlayerEventArgs(newPlayer));
            
        Assert.Empty(_sut.Players);
    }

    [Fact]
    public void GameOnStyleChangedChangesCurrentPlayerSelectedStyle()
    {
        _sut.AttachHandlers();
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            
        var remotePlayer = new Player(PlayerType.Network){ SelectedStyle = DiceStyle.Blue};
            
        _gameService.CurrentLocalGame.StyleChanged +=
            Raise.EventWith(null, new PlayerEventArgs(remotePlayer));
            
        Assert.Equal(remotePlayer.SelectedStyle, _humanPlayer.SelectedStyle);
    }
        
    [Fact]
    public void GameOnStyleChangedDoesNotChangeCurrentPlayerSelectedStyleIfThereIsNoCurrentPlayer()
    {
        _sut.AttachHandlers();

        var remotePlayer = new Player(PlayerType.Network){ SelectedStyle = DiceStyle.Blue};
            
        _gameService.CurrentLocalGame.StyleChanged +=
            Raise.EventWith(null, new PlayerEventArgs(remotePlayer));
            
        Assert.NotEqual(remotePlayer.SelectedStyle, _humanPlayer.SelectedStyle);
    }
            
    [Fact]
    public void GameOnStyleChangedDoesNotChangeCurrentPlayerSelectedStyleIfViewIsNotActive()
    {
        _sut.AttachHandlers();
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);

        var defaultStyle = _humanPlayer.SelectedStyle;
            
        var remotePlayer = new Player(PlayerType.Network){ SelectedStyle = DiceStyle.Blue};
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.StyleChanged +=
            Raise.EventWith(null, new PlayerEventArgs(remotePlayer));
            
        Assert.Equal(defaultStyle, _humanPlayer.SelectedStyle);
    }

    [Fact]
    public void GameOnResultAplliedPlaysSadSoundIfValueIsZeroAndNotBonus()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 0, Scores.Ones,false));
            
        _soundsProvider.Received().PlaySound("wrong");
    }
        
    [Fact]
    public void GameOnResultAplliedPlaysWinSoundIfValueIsNotZeroAndNotBonus()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 40, Scores.LargeStraight, false));
            
        _soundsProvider.Received().PlaySound("win");
    }
        
    [Fact]
    public void GameOnResultAplliedPlaysFanfareSoundIfValueIsYatzy()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 50, Scores.Kniffel,false));
            
        _soundsProvider.Received().PlaySound("fanfare");
    }
        
    [Fact]
    public void GameOnResultAplliedPlaysFanfareSoundInCaseOfBonus()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 40, Scores.LargeStraight, true));
            
        _soundsProvider.Received().PlaySound("fanfare");
    }
        
    [Fact]
    public void GameOnResultAplliedRefreshesGameStatus()
    {
        var testAction = new Action(() =>
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.ResultApplied +=
                Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 40, Scores.LargeStraight, false));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }
        
    [Fact]
    public void GameOnResultAplliedDoesNotRefreshGameStatusIfThereIsNoCurrentPlayer()
    {
        var testAction = new Action(() =>
        {
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.ResultApplied +=
                Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 40, Scores.LargeStraight, false));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction,0);
    }

    [Fact]
    public void GameOnResultRemovesCurrentRollResults()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        Assert.NotNull(_sut.RollResults);
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 40, Scores.LargeStraight, true));

        Assert.Null(_sut.RollResults);
    }

    [Fact]
    public void GameOnResultAplliedDoesNotPlayAnySoundIfViewIsNotActive()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.ResultApplied +=
            Raise.EventWith(null, new RollResultEventArgs(_humanPlayer, 0, Scores.Ones, false));
            
        _soundsProvider.DidNotReceiveWithAnyArgs().PlaySound("wrong");
    }

    [Fact]
    public void GameOnRerolledPlaysMagicSound()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.PlayerRerolled +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _soundsProvider.Received().PlaySound("magic");
    }

    [Fact]
    public void GameOnRerolledConsumesArtifact()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.PlayerRerolled +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _humanPlayer.Received().UseArtifact(Artifacts.RollReset);
    }
        
    [Fact]
    public void GameOnRerolledClearsCurrentRollResults()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        var results = new[] {2, 4, 6, 2, 1};
        _sut.AttachHandlers();

        _gameService.CurrentLocalGame.DiceChanged +=
            Raise.EventWith(null, new RollEventArgs(_humanPlayer, results));

        Assert.NotNull(_sut.RollResults);
            
        _gameService.CurrentLocalGame.PlayerRerolled +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        Assert.Null(_sut.RollResults);
    }
        
    [Fact]
    public void GameOnRerolledRefreshesGameStatus()
    {
        var testAction = new Action(() =>
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.PlayerRerolled +=
                Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }
        
    [Fact]
    public void GameOnRerolledDoesNotRefreshGameStatusIfThereIsNoCurrentPlayer()
    {
        var testAction = new Action(() =>
        {
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.PlayerRerolled +=
                Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction,0);
    }
        
    [Fact]
    public void GameOnRerolledDoesNotPlayMagicSoundIfViewIsNotActive()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.PlayerRerolled +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _soundsProvider.DidNotReceiveWithAnyArgs().PlaySound("magic");
    }

    [Fact]
    public void GameOnMagicRollPlaysMagicSound()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.MagicRollUsed +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _soundsProvider.Received().PlaySound("magic");
    }
        
    [Fact]
    public void GameOnMagicRollConsumesArtifact()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.MagicRollUsed +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _humanPlayer.Received().UseArtifact(Artifacts.MagicalRoll);
    }
        
    [Fact]
    public void GameOnMagicRollRefreshesGameStatus()
    {
        var testAction = new Action(() =>
        {
            _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.MagicRollUsed +=
                Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }
        
    [Fact]
    public void GameOnMagicRollDoesNotRefreshGameStatusIfThereIsNoCurrentPlayer()
    {
        var testAction = new Action(() =>
        {
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            
            _gameService.CurrentLocalGame.MagicRollUsed +=
                Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction,0);
    }
        
    [Fact]
    public void GameOnMagicRollDoesNotPlayMagicSoundIfViewIsNotActive()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        _gameService.CurrentLocalGame.MagicRollUsed +=
            Raise.EventWith(null, new PlayerEventArgs(_humanPlayer));
            
        _soundsProvider.DidNotReceiveWithAnyArgs().PlaySound("magic");
    }

    [Fact]
    public void AmountOfResultsLabelsMatchesAmountOfRollResultsForEveryRule()
    {
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.IsHuman.Returns(true);
        foreach (var rule in EnumUtils.GetValues<Rules>())
        {
            _sut.DetachHandlers();
                
            _gameService.CurrentLocalGame.Rules.Returns(new Rule(rule));
            IReadOnlyList<RollResult> rollResults = _gameService.CurrentLocalGame.Rules.ScoresForRule
                .Select(score => new RollResult(score, rule)).ToList();
            _sut.AttachHandlers();

            Assert.NotEmpty(_sut.RollResultsLabels);
            Assert.Equal(rollResults.Count, _sut.RollResultsLabels.Count);
            Assert.NotEmpty(_sut.RollResultsShortLabels);
            Assert.Equal(rollResults.Count, _sut.RollResultsShortLabels.Count);
        }
    }
        
    [Fact]
    public void PanelTitlesAreCorrect()
    {
        _localizationService.GetLocalizedString("ResultsTableLabel").Returns(Strings.ResultsTableLabel);
        _localizationService.GetLocalizedString("DiceBoardLabel").Returns(Strings.DiceBoardLabel);
        
        Assert.Equal(Strings.ResultsTableLabel.ToUpper(), _sut.ScoresTitle);
        Assert.Equal(Strings.DiceBoardLabel.ToUpper(), _sut.PanelTitle);
    }

    [Fact]
    public void ButtonImagesAreCorrect()
    {
        _sut.RollImage.Should().Be("Roll.png");
        _sut.MagicRollImage.Should().Be("MagicRoll.png");
        _sut.ManualSetImage.Should().Be("ManualSet.png");
        _sut.RollResetImage.Should().Be("RollReset.png");
        _sut.CloseImage.Should().Be("Close.png");
    }

    [Fact]
    public void RollCommandCallsReportRollOnGame()
    {
        // Arrange
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
            
        // Act
        _sut.RollCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.Received().ReportRoll();
    }
        
    [Fact]
    public void RollCommandDoesNotCallReportRollOnGameWhenCantRoll()
    {
        // Arrange
        _sut.AttachHandlers();
            
        // Act
        _sut.RollCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.DidNotReceive().ReportRoll();
    }
        
    [Fact]
    public void MagicRollCommandCallsCorrespondingGameMethod()
    {
        // Arrange
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new[]
        {
            new Artifact(Artifacts.MagicalRoll)
        });
        _sut.AttachHandlers();
            
        // Act
        _sut.MagicRollCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.Received().ReportMagicRoll();
    }
        
    [Fact]
    public void MagicRollCommandDoesNotCallCorrespondingGameMethodWhenItsNotPossible()
    {
        // Arrange
        _sut.AttachHandlers();
            
        // Act
        _sut.MagicRollCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.DidNotReceive().ReportMagicRoll();
    }

    [Fact]
    public void ManualSetCommandChangesDicePanelManualSetModeToOn()
    {
        // Arrange
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new[]
        {
            new Artifact(Artifacts.ManualSet)
        });
        _sut.AttachHandlers();
            
        // Act
        _sut.ManualSetCommand.Execute(null);
            
        // Assert
        Assert.True(_dicePanel.ManualSetMode);
    }
        
    [Fact]
    public void ManualSetCommandDoesNotChangeDicePanelManualSetModeWhenItsNotPossible()
    {
        // Arrange
        _sut.AttachHandlers();
            
        // Act
        _sut.ManualSetCommand.Execute(null);
            
        // Assert
        Assert.False(_dicePanel.ManualSetMode);
    }
        
    [Fact]
    public void RollResetCommandCallsCorrespondingGameMethod()
    {
        // Arrange
        _gameService.CurrentLocalGame.Rules.Returns(new Rule(Rules.krMagic));
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.MagicalArtifactsForGame.Returns(new[]
        {
            new Artifact(Artifacts.RollReset)
        });
        _sut.AttachHandlers();
            
        // Act
        _sut.RollResetCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.Received().ResetRolls();
    }
        
    [Fact]
    public void RollResetCommandDoesNotCallCorrespondingGameMethodWhenItsNotPossible()
    {
        // Arrange
        _sut.AttachHandlers();
            
        // Act
        _sut.RollResetCommand.Execute(null);
            
        // Assert
        _gameService.CurrentLocalGame.DidNotReceive().ResetRolls();
    }

    [Fact]
    public void TitleContainsRollNumber()
    {
        _humanPlayer.IsHuman.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        for (var roll = 1; roll < 4; roll++)
        {
            _gameService.CurrentLocalGame.Roll.Returns(roll);
            Assert.Contains(roll.ToString(), _sut.Title);
        }
    }
 
    [Fact]
    public void DicePanelOnRollEndedDoesNotUpdateCurrentPlayerRollIfViewIsNotActive()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll = 1;
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();
        _sut.DetachHandlers();

        _dicePanel.RollEnded +=
            Raise.Event();
            
        Assert.Equal(1,_humanPlayer.Roll);
    }
    
    [Fact]
    public void DicePanelOnRollEnded_DoesNotUpdateCurrentPlayerRollResults_IfGameHasNotStarted()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll = 1;
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        _dicePanel.RollEnded +=
            Raise.Event();

        _sut.RollResults.Should().BeNull();
    }
    
    [Fact]
    public void DicePanelOnRollEnded_ShouldShowRollForHuman_IfGameHasNotStarted()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll = 1;
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        _dicePanel.RollEnded +=
            Raise.Event();

        _sut.CanRoll.Should().BeTrue();
    }
        
    [Fact]
    public void DicePanelOnRollEndedDoesNotRefreshGameStatusIfThereIsNoCurrentPlayer()
    {
        var testAction = new Action(() =>
        {
            _dicePanel.RollEnded +=
                Raise.Event();
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction, 0);
    }
        
    [Fact]
    public void DicePanelOnRollEndedForcesBotPlayerToDecideFillOnTheLastRoll()
    {
        var decisionMaker = Substitute.For<IGameDecisionMaker>();
        _botPlayer.DecisionMaker.Returns(decisionMaker);
        _botPlayer.IsBot.Returns(true);
        _botPlayer.IsHuman.Returns(false);
        _botPlayer.Roll = 3;
        decisionMaker.NeedsToRollAgain().Returns(true);
        _botPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple)});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
            
        _dicePanel.RollEnded +=
            Raise.Event();
            
        decisionMaker.Received().DecideFill(_gameService.CurrentLocalGame);
    }
        
    [Fact]
    public void DicePanelOnRollEndedForcesBotPlayerToDecideFillIfHeDoesNotHaveToRollAgain()
    {
        var decisionMaker = Substitute.For<IGameDecisionMaker>();
        _botPlayer.DecisionMaker.Returns(decisionMaker);
        _botPlayer.IsBot.Returns(true);
        _botPlayer.IsHuman.Returns(false);
        decisionMaker.NeedsToRollAgain().Returns(false);
        _botPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple)});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
            
        _dicePanel.RollEnded +=
            Raise.Event();
            
        decisionMaker.Received().DecideFill(_gameService.CurrentLocalGame);
    }
        
    [Fact]
    public void DicePanelOnRollEndedForcesBotToTryAndFixDiceIfNotLastRoll()
    {
        var decisionMaker = Substitute.For<IGameDecisionMaker>();
        _botPlayer.DecisionMaker.Returns(decisionMaker);
        _botPlayer.IsBot.Returns(true);
        _botPlayer.IsHuman.Returns(false);
        _botPlayer.Roll = 2;
        decisionMaker.NeedsToRollAgain().Returns(true);
        _botPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple)});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _sut.AttachHandlers();
            
        _dicePanel.RollEnded +=
            Raise.Event();
            
        decisionMaker.Received().FixDice(_gameService.CurrentLocalGame);
    }
        
    [Fact]
    public void DicePanelOnRollEndedForcesBotToFillIfNotLastRollAndAllDiceAreFixed()
    {
        var decisionMaker = Substitute.For<IGameDecisionMaker>();
        _botPlayer.DecisionMaker.Returns(decisionMaker);
        _botPlayer.IsBot.Returns(true);
        _botPlayer.IsHuman.Returns(false);
        _botPlayer.Roll = 2;
        decisionMaker.NeedsToRollAgain().Returns(true);
        _botPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple)});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _gameService.CurrentLocalGame.NumberOfFixedDice.Returns(5);
        _sut.AttachHandlers();
            
        _dicePanel.RollEnded +=
            Raise.Event();
            
        decisionMaker.Received().DecideFill(_gameService.CurrentLocalGame);
    }
        
    [Fact]
    public void DicePanelOnRollEndedForcesBotToDecideNextRollIfNotLastRollAndNotAllDiceAreFixed()
    {
        var decisionMaker = Substitute.For<IGameDecisionMaker>();
        _botPlayer.DecisionMaker.Returns(decisionMaker);
        _botPlayer.IsBot.Returns(true);
        _botPlayer.IsHuman.Returns(false);
        _botPlayer.Roll = 2;
        decisionMaker.NeedsToRollAgain().Returns(true);
        _botPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple)});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
        _gameService.CurrentLocalGame.NumberOfFixedDice.Returns(3);
        _sut.AttachHandlers();
            
        _dicePanel.RollEnded +=
            Raise.Event();
            
        decisionMaker.Received().DecideRoll(_gameService.CurrentLocalGame, _dicePanel);
    }
        
    [Fact]
    public void DicePanelOnRollEndedPopulatesRollResults()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll = 1;
        _humanPlayer.Results.Returns(new List<RollResult>() {new RollResult(Scores.Ones, Rules.krSimple){PossibleValue = 5}});
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);
        _sut.AttachHandlers();

        _dicePanel.RollEnded +=
            Raise.Event();
            
        Assert.NotNull(_sut.RollResults);
        Assert.NotEmpty(_sut.RollResults);
    }
        
    [Fact]
    public void DicePanelOnRollRefreshesGameStatus()
    {
        _humanPlayer.IsHuman.Returns(true);
        _humanPlayer.Roll = 1;
        _humanPlayer.Results.Returns(new IRollResult[] { new RollResult(Scores.Fives, Rules.krBaby){PossibleValue = 5} });
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_humanPlayer);

        var testAction = new Action(() =>
        {
            if (!_sut.Players.Any())
                _sut.AttachHandlers();
            _dicePanel.RollEnded +=
                Raise.Event();
        });
            
        CheckIfGameStatusHasBeenRefreshed(testAction);
    }

    [Fact]
    public void ApplyRollResultCallsCorrespondingMethodOnCurrentPlayer()
    {
        var rollResult = new RollResult(Scores.Ones, Rules.krSimple);
        _sut.AttachHandlers();

        _sut.ApplyRollResult(rollResult);
            
        _gameService.CurrentLocalGame.Received().ApplyScore(rollResult);
    }

    [Fact]
    public void FixDiceOnDicePanelNotifiesGame()
    {
        const int diceValue = 2;
        const bool isFixed = true;
        _sut.AttachHandlers();
            
        _dicePanel.DieFixed +=
            Raise.EventWith(null, new DiceFixedEventArgs(isFixed,diceValue));

        _gameService.CurrentLocalGame.Received().FixDice(diceValue,isFixed);
    }
        
    [Fact]
    public void FixDiceOnDicePanelDoesNotNotifyGameWhenViewIsNotActive()
    {
        const int diceValue = 2;
        const bool isFixed = true;
        _sut.AttachHandlers();
        _sut.DetachHandlers();
            
        _dicePanel.DieFixed +=
            Raise.EventWith(null, new DiceFixedEventArgs(isFixed,diceValue));

        _gameService.CurrentLocalGame.DidNotReceive().FixDice(diceValue,isFixed);
    }

    [Fact]
    public void BotStartsTheGameIfItIsTheFirstPlayer()
    {
        _humanPlayer.IsHuman.Returns(true);
        _botPlayer.IsBot.Returns(true);
        _gameService.CurrentLocalGame.CurrentPlayer.Returns(_botPlayer);
            
        _sut.AttachHandlers();
            
        _gameService.CurrentLocalGame.Received().ReportRoll();
    }

    [Fact]
    public void MagicRollButtonLabel_IsReturnedFromTheLocalizationService()
    {
        const string magicRoll = "Magic Roll";
        _localizationService.GetLocalizedString("MagicRollLabel").Returns(magicRoll);

        var result = _sut.MagicRollLabel;

        result.Should().Be(magicRoll);
    }
    
    [Fact]
    public void ManualSetButtonLabel_IsReturnedFromTheLocalizationService()
    {
        const string expected = "Manual Set";
        _localizationService.GetLocalizedString("ManualSetLabel").Returns(expected);

        var result = _sut.ManualSetLabel;

        result.Should().Be(expected);
    }
    
    [Fact]
    public void RollResetButtonLabel_IsReturnedFromTheLocalizationService()
    {
        const string expected = "Roll Reset";
        _localizationService.GetLocalizedString("RollResetLabel").Returns(expected);

        var result = _sut.RollResetLabel;

        result.Should().Be(expected);
    }
    
    [Fact]
    public void TotalLabel_IsReturnedFromTheLocalizationService()
    {
        const string expected = "Total";
        _localizationService.GetLocalizedString("PlayerTotalScoreLabel").Returns(expected);

        var result = _sut.TotalLabel;

        result.Should().Be(expected);
    }
    
    [Fact]
    public void TotalShortLabel_IsReturnedFromTheLocalizationService()
    {
        const string expected = "T!";
        _localizationService.GetLocalizedString("TotalShort").Returns(expected);

        var result = _sut.TotalShortLabel;

        result.Should().Be(expected);
    }

    [Fact]
    public void GettingSelectedRollResult_IsNull()
    {
        _sut.SelectedRollResult.Should().BeNull();
    }

    [Fact]
    public void SettingSelectedRollResult_AppliesItToGame()
    {
        var rollResult = new RollResultViewModel(_rollResult, _localizationService);
        _sut.SelectedRollResult = rollResult;
        
        _gameService.CurrentLocalGame.Received().ApplyScore(_rollResult);
    }
    
    #region Private methods

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void CheckIfGameStatusHasBeenRefreshed(Action testAction, int expectedTimesExecuted = 1)
    {
        var currentPlayerUpdated = 0;
        var rollLabelUpdatedTimes = 0;
        var canRollUpdatedTimes = 0;
        var titleUpdatedTimes = 0;
        var playerResultsRefreshedTimes = 0;

        var magicRollVisibilityCheckedTimes = 0;
        var fourthRollVisibilityCheckedTimes = 0;
        var manualSetRollVisibilityCheckedTimes = 0;
            
        _sut.AttachHandlers();
            
        _sut.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(_sut.CurrentPlayer):
                    currentPlayerUpdated++;
                    break;
                case nameof(_sut.RollLabel):
                    rollLabelUpdatedTimes++;
                    break;
                case nameof(_sut.Title):
                    titleUpdatedTimes++;
                    break;
                case nameof(_sut.IsMagicRollVisible):
                    magicRollVisibilityCheckedTimes++;
                    break;
                case nameof(_sut.IsManualSetVisible):
                    manualSetRollVisibilityCheckedTimes++;
                    break;
                case nameof(_sut.IsRollResetVisible):
                    fourthRollVisibilityCheckedTimes++;
                    break;
                case nameof(_sut.CanRoll):
                    canRollUpdatedTimes++;
                    break;
            }
        };
            
        foreach (var playerViewModel in _sut.Players)
        {
            playerViewModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(playerViewModel.Results):
                        playerResultsRefreshedTimes++;
                        break;
                }
            };
        }

        testAction();

        Assert.Equal(_sut.CanFix, _dicePanel.ClickToFix);
            
        Assert.Equal(expectedTimesExecuted,currentPlayerUpdated);
        Assert.Equal(expectedTimesExecuted, rollLabelUpdatedTimes);
        Assert.Equal(expectedTimesExecuted,canRollUpdatedTimes);
        Assert.Equal(expectedTimesExecuted, titleUpdatedTimes);
            
        if (expectedTimesExecuted == 0)
            return;
            
        if (_sut.HasCurrentPlayer)
            Assert.Equal(_sut.Players.Count, playerResultsRefreshedTimes);

        if (_sut.Game.Rules.CurrentRule != Rules.krMagic) return;
        Assert.True(magicRollVisibilityCheckedTimes>0);
        Assert.True(fourthRollVisibilityCheckedTimes>0);
        Assert.True(manualSetRollVisibilityCheckedTimes>0);
    }
    #endregion  
}