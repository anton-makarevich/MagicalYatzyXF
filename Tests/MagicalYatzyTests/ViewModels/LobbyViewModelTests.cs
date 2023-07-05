using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;
using Sanet.MagicalYatzy.Resources;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MagicalYatzyTests.ViewModels
{
    public class LobbyViewModelTests
    {
        private readonly LobbyViewModel _sut;
        private readonly IPlayerService _playerService;
        private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
        private readonly IRulesService _rulesService = Substitute.For<IRulesService>();
        private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
        private readonly IGameService _gameService = Substitute.For<IGameService>();

        public LobbyViewModelTests()
        {
            _playerService = Substitute.For<IPlayerService>();
            var dicePanelMock = Substitute.For<IDicePanel>();
            _sut = new LobbyViewModel(
                dicePanelMock, 
                _playerService, 
                _rulesService, 
                _gameService,
                _localizationService);
        }

        [Fact]
        public void LobbyViewModelHasDicePanel()
        {
            Assert.NotNull(_sut.DicePanel);
        }

        [Fact]
        public void PanelTitles_AreCorrect()
        {
            _localizationService.GetLocalizedString("PlayersLabel").Returns(Strings.PlayersLabel);
            _localizationService.GetLocalizedString("RulesLabel").Returns(Strings.RulesLabel);
            
            _sut.PlayersTitle.Should().Be(Strings.PlayersLabel.ToUpper());
            _sut.RulesTitle.Should().Be(Strings.RulesLabel.ToUpper());
        }
        
        [Fact]
        public void ActionButtonLabels_AreCorrect()
        {
            _localizationService.GetLocalizedString("StartGameButton").Returns(Strings.StartGameButton);
            _localizationService.GetLocalizedString("AddBotLabel").Returns(Strings.AddBotLabel);
            _localizationService.GetLocalizedString("AddPlayerLabel").Returns(Strings.AddPlayerLabel);
            
            _sut.StartTitle.Should().Be(Strings.StartGameButton);
            _sut.AddBotLabel.Should().Be(Strings.AddBotLabel);
            _sut.AddPlayerLabel.Should().Be(Strings.AddPlayerLabel);
        }

        [Fact]
        public void CurrentLoggedInPlayerAddedToPlayersList()
        {
            _sut.AttachHandlers();
            
            Assert.NotEmpty(_sut.Players);
            Assert.Equal(_playerService.CurrentPlayer.Name, _sut.Players.First().Name);
        }
        
        [Fact]
        public void CallingDeleteOnPlayerViewModelRemovesItFromList()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>(), _localizationService);

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            var initialPlayerCount = _sut.Players.Count;
            
            newPlayerVm.DeleteCommand.Execute(null);

            Assert.Equal(initialPlayerCount - 1, _sut.Players.Count);
        }
        
        [Fact]
        public void SinglePlayerCanNotBeDeleted()
        {
            _sut.AttachHandlers();

            Assert.Single(_sut.Players);
            Assert.False(_sut.Players.First().CanBeDeleted);
        }

        [Fact]
        public void PlayersCouldBeDeletedIfThereAreMoreThanTwoInTheList()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>(), _localizationService);

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            
            Assert.Equal(2, _sut.Players.Count);
            foreach (var playerViewModel in _sut.Players)
            {
                Assert.True(playerViewModel.CanBeDeleted);
            }
        }
        
        [Fact]
        public void PlayersCanNotBeDeletedIfOnlyOneRemainsAfterDeletion()
        {
            var newPlayerVm = new PlayerViewModel(Substitute.For<IPlayer>(), _localizationService);

            _sut.AttachHandlers();
            _sut.AddPlayer(newPlayerVm);
            newPlayerVm.DeleteCommand.Execute(null);
            
            Assert.Single(_sut.Players);
            Assert.False(_sut.Players.First().CanBeDeleted);
        }

        [Fact]
        public void AddBotImageHasCorrectValue()
        {
            Assert.Equal("AddBot.png", _sut.AddBotImage);
        }
        
        [Fact]
        public void AddPlayerImageHasCorrectValue()
        {
            Assert.Equal("AddPlayer.png", _sut.AddPlayerImage);
        }
        
        [Fact]
        public void StartImageHasCorrectValue()
        {
            Assert.Equal("Start.png", _sut.StartImage);
        }

        [Fact]
        public void AddBotCommandAddsNewPlayer()
        {
            _sut.AddBotCommand.Execute(null);
            
            Assert.Single(_sut.Players);
        }

        [Fact]
        public void AddBotCommandAddsAiPlayer()
        {
            _sut.AddBotCommand.Execute(null);
            
            Assert.Equal("BotPlayer.png", _sut.Players.First().Image);
            Assert.True(_sut.Players.First().Player.IsBot);
        }
        
        [Fact]
        public void NextAddedBotHasIncreasedNumber()
        {
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.Equal('2', _sut.Players.Last().Name.Last());
        }

        [Fact]
        public void BotsDontHaveTheSameName()
        {
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.Players.First().DeleteCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.NotEqual(_sut.Players.First().Name, _sut.Players.Last().Name);
        }

        [Fact]
        public void ItsNotPossibleToAddBotIfThereAreAlreadyFourPlayers()
        {
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            Assert.Equal(4,_sut.Players.Count);
            
            _sut.AddBotCommand.Execute(null);
            Assert.Equal(4,_sut.Players.Count);
        }
        
        [Fact]
        public void CanAddBotIsFalseIfThereAreAlreadyFourPlayers()
        {
            var canAddBotChanged = false;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.CanAddBot)) canAddBotChanged = true;
            };
            
            Assert.True(_sut.CanAddBot);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.False(_sut.CanAddBot);
            Assert.True(canAddBotChanged);
        }
        
        [Fact]
        public void CanAddBotIsTrueAgainWhenPlayerIsRemoved()
        {
            var addBotChangedCalled = 0;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.CanAddBot)) addBotChangedCalled++;
            };
            
            Assert.True(_sut.CanAddBot);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.False(_sut.CanAddBot);
            
            _sut.Players.Last().DeleteCommand.Execute(null);
            Assert.True(_sut.CanAddBot);
            
            Assert.Equal(2,addBotChangedCalled);
        }

        [Fact]
        public void AddHumanCommandAddsPlayer()
        {
            // Arrange
            var playerStub = Substitute.For<IPlayer>();
            _navigationService.ShowViewModelForResultAsync<LoginViewModel, IPlayer>()
                .Returns(Task.FromResult(playerStub));
            _sut.SetNavigationService(_navigationService);
            
            // Act
            _sut.AddHumanCommand.Execute(null);
            
            // Assert
            Assert.Single(_sut.Players);
        }

        [Fact(Skip = "To be implemented")]
        public void AddHumanCommandDoesNotAddPlayerIfNullIsReturned()
        {
            // Arrange
            _navigationService.ShowViewModelForResultAsync<LoginViewModel, IPlayer>()
                .Returns(Task.FromResult<IPlayer>(null));
            _sut.SetNavigationService(_navigationService);

            // Act
            _sut.AddHumanCommand.Execute(null);

            // Assert
            Assert.Empty(_sut.Players);
        }

        [Fact]
        public void ItsNotPossibleToAddHumanIfThereAreAlreadyFourPlayers()
        {
            // Arrange
            var playerStub = Substitute.For<IPlayer>();
            _navigationService.ShowViewModelForResultAsync<LoginViewModel, IPlayer>()
                .Returns(Task.FromResult(playerStub));
            _sut.SetNavigationService(_navigationService);
            
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            Assert.Equal(4,_sut.Players.Count);
            
            _sut.AddHumanCommand.Execute(null);
            Assert.Equal(4,_sut.Players.Count);
        }
        
        [Fact]
        public void CanAddHumanIsFalseIfThereAreAlreadyFourPlayers()
        {
            var canAddHumanChanged = false;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.CanAddHuman)) canAddHumanChanged = true;
            };
            
            Assert.True(_sut.CanAddHuman);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.False(_sut.CanAddHuman);
            Assert.True(canAddHumanChanged);
        }
        
        [Fact]
        public void CanAddHumanIsTrueAgainWhenPlayerIsRemoved()
        {
            var addHumanChangedCalled = 0;
            _sut.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_sut.CanAddHuman)) addHumanChangedCalled++;
            };
            
            Assert.True(_sut.CanAddHuman);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.False(_sut.CanAddHuman);
            
            _sut.Players.Last().DeleteCommand.Execute(null);
            Assert.True(_sut.CanAddHuman);
            
            Assert.Equal(2,addHumanChangedCalled);
        }

        [Fact]
        public void LoadRulesLoadsRules()
        {
            _rulesService.GetAllRules().Returns(new []{Rules.krBaby, Rules.krMagic});
            
            _sut.LoadRules();
            
            Assert.NotEmpty(_sut.Rules);
        }

        [Fact]
        public void RulesAreLoadedOnPageAppear()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krMagic });

            _sut.AttachHandlers();

            Assert.NotEmpty(_sut.Rules);
        }

        [Fact]
        public void RulesAreLoadedOnlyOnce()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krMagic });

            _sut.LoadRules();
            _sut.LoadRules();

            Assert.Equal(2, _sut.Rules.Count);
        }

        [Fact]
        public void DefaultRuleIsSelectedWhenRulesAreLoaded()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();

            Assert.Single(_sut.Rules.Where(r => r.IsSelected));
            Assert.Equal(Rules.krSimple, _sut.SelectedRule?.Rule);
        }

        [Fact]
        public void ClearsRulesOnViewDisappear()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            
            _sut.DetachHandlers();
            
            Assert.Empty(_sut.Rules);
        }
        
        [Fact]
        public void RuleSelectionChangesSelectedRule()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();

            var babyRule = _sut.Rules.FirstOrDefault(f => f.Rule == Rules.krBaby);
            babyRule?.SelectRuleCommand?.Execute(null);

            Assert.Single(_sut.Rules.Where(r => r.IsSelected));
            Assert.Equal(babyRule?.Rule, _sut.SelectedRule?.Rule);
        }
        
        [Fact]
        public void SelectedRuleUpdate_ChangesSelectedRule()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();

            var babyRule = _sut.Rules.FirstOrDefault(f => f.Rule == Rules.krBaby);
            _sut.SelectedRule = babyRule;
            
            babyRule?.IsSelected.Should().BeTrue();
        }

        [Fact]
        public void SettingSelectedRuleToNull_DoesNotBreakCode()
        {
            _sut.SelectedRule = null;
        }

        [Fact]
        public async Task StartNewGameCallsStartNewGameWithSelectedRule()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            _sut.SetNavigationService(_navigationService);
            var rule = _sut.Rules.First();
            rule.SelectRuleCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            _sut.StartGameCommand.Execute(null);

            await _gameService.Received().CreateNewLocalGameAsync(rule.Rule);
        }
        
        [Fact]
        public async Task StartNewGameAddsPlayersToCurrentGame()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.AddBotCommand.Execute(null);
            var playerStub = new Player();
            _navigationService.ShowViewModelForResultAsync<LoginViewModel, IPlayer>()
                .Returns(Task.FromResult<IPlayer>(playerStub));
            _sut.SetNavigationService(_navigationService);
            _sut.AddHumanCommand.Execute(null);
            _sut.LoadRules();
            var rule = _sut.Rules.First();
            rule.SelectRuleCommand.Execute(null);
            
            var game = new YatzyGame(rule.Rule, Substitute.For<IDiceGenerator>());
            _gameService.CreateNewLocalGameAsync(rule.Rule)
                .Returns(Task.FromResult<IGame>(game));
            
            _sut.StartGameCommand.Execute(null);

            await _gameService.Received().CreateNewLocalGameAsync(rule.Rule);
            
            Assert.Equal(2,game.NumberOfPlayers);
            foreach (var playerViewModel in _sut.Players)
            {
                Assert.Contains(playerViewModel.Player, game.Players);
            }
        }
        
        [Fact]
        public async Task StartNewGameNavigatesToGameViewModel()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            var rule = _sut.Rules.First();
            rule.SelectRuleCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            _sut.SetNavigationService(_navigationService);
            
            _sut.StartGameCommand.Execute(null);

            await _navigationService.Received().NavigateToViewModelAsync<GameViewModel>();
        }
        
        [Fact]
        public async Task StartNewGameDoesNotNavigateToGameViewModelIfNoPlayersAreAdded()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            var rule = _sut.Rules.First();
            rule.SelectRuleCommand.Execute(null);
            _sut.SetNavigationService(_navigationService);
            
            _sut.StartGameCommand.Execute(null);

            await _navigationService.DidNotReceive().NavigateToViewModelAsync<GameViewModel>();
        }
        
        [Fact]
        public async Task StartNewGameDoesNotNavigateToGameViewModelIfRuleIsNotSelected()
        {
            _sut.AddBotCommand.Execute(null);
            _sut.SetNavigationService(_navigationService);
            
            _sut.StartGameCommand.Execute(null);

            await _navigationService.DidNotReceive().NavigateToViewModelAsync<GameViewModel>();
        }

        [Fact]
        public void CannotStartGameIfRuleIsNotSelected()
        {
            _sut.AddBotCommand.Execute(null);
            
            Assert.False(_sut.CanStartGame);
        }
        
        [Fact]
        public void CannotStartGameIfThereAreNoPlayers()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            _sut.Rules.First().SelectRuleCommand.Execute(null);
            
            Assert.False(_sut.CanStartGame);
        }
        
        [Fact]
        public void CanStartGameIfPlayersAreAddedAndRuleIsSelected()
        {
            _rulesService.GetAllRules().Returns(new[] { Rules.krBaby, Rules.krSimple });
            _sut.LoadRules();
            _sut.Rules.First().SelectRuleCommand.Execute(null);
            _sut.AddBotCommand.Execute(null);
            
            Assert.True(_sut.CanStartGame);
        }

        [Fact]
        public void DoesNotAddMorePlayerWhenMaxAmountIsReached()
        {
            for (var index = 0; index <= LobbyViewModel.MaxPlayers+2; index++)
            {
                _sut.AddPlayer(new PlayerViewModel(
                    Substitute.For<IPlayer>(),
                    Substitute.For<ILocalizationService>()));
            }
            
            Assert.Equal(LobbyViewModel.MaxPlayers, _sut.Players.Count);
        }
    }
}
