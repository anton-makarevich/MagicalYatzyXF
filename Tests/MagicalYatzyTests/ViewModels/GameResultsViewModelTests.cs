using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels;
using Sanet.MVVM.Core.Services;
using Xunit;

namespace MagicalYatzyTests.ViewModels;

public class GameResultsViewModelTests
{
    private readonly IGameService _gameService = Substitute.For<IGameService>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly IApiClient _apiClient = Substitute.For<IApiClient>();
    private readonly IPlayer _humanPlayer;
    private readonly IGame _game = Substitute.For<IGame>();
        
    private readonly GameResultsViewModel _sut;

    public GameResultsViewModelTests()
    {
        _humanPlayer = Substitute.For<IPlayer>();
        _humanPlayer.InGameId.Returns("0");
        _humanPlayer.Roll = 1;

        _gameService.CurrentLocalGame.Returns(_game);
        _game.Rules.Returns(new Rule(Rules.krSimple));
        _game.Players.Returns(new List<IPlayer>()
        {
            _humanPlayer,
        });
            
        Substitute.For<ILocalizationService>();
        _sut = new GameResultsViewModel(_gameService, _localizationService, _apiClient);
        _sut.SetNavigationService(_navigationService);
    }
        
    [Fact]
    public void PopulatesListOfPlayersOnAppear()
    {
        _sut.AttachHandlers();
            
        Assert.NotNull(_sut.Players);
        Assert.NotEmpty(_sut.Players);
    }

    [Fact]
    public async Task RestartGameCreatesNewCurrentGameWithTheSamePlayers()
    {
        var newGame = Substitute.For<IGame>();
        _gameService.CreateNewLocalGameAsync(Arg.Any<Rules>()).Returns(Task.FromResult(newGame));
            
        _sut.RestartGameCommand.Execute(null);

        await _gameService.Received().CreateNewLocalGameAsync(Rules.krSimple);
        newGame.Received().JoinGame(_humanPlayer);
    }
        
    [Fact]
    public async Task RestartGameNavigatesToGameView()
    {
        _sut.RestartGameCommand.Execute(null);

        await _navigationService.Received().NavigateToViewModelAsync<GameViewModel>();
    }

    [Fact]
    public void RestartImageHasCorrectValue()
    {
        Assert.Equal("PlayAgain.png" ,_sut.RestartImage);
    }

    [Fact]
    public async Task CloseShouldNavigateToTheRoot()
    {
        _sut.CloseCommand.Execute(null);

        await _navigationService.Received().NavigateToRootAsync();
    }

    [Fact]
    public void CloseImageHasCorrectValue()
    {
        Assert.Equal("Close.png", _sut.CloseImage);
    }

    [Fact]
    public void DoesNotCreatePlayersCollectionIfThereIsNoPlayersInGame()
    {
        _game.Players.Returns(new List<IPlayer>());
        _sut.AttachHandlers();
            
        Assert.Null(_sut.Players);
    }

    [Fact]
    public async Task SavesScoreForEveryHumanPlayerOnAppear()
    {
        const int botsCount = 2;
        const int humanPlayersCount = 4;
            
        var botPlayers = new List<IPlayer>();
        for (var i = 0; i < botsCount; i++)
        {
            var botPlayer = Substitute.For<IPlayer>();
            botPlayer.IsBot.Returns(true);
            botPlayer.IsHuman.Returns(false);
            botPlayers.Add(botPlayer);
        }
            
        var humanPlayers = new List<IPlayer>();
        for (var i = 0; i < humanPlayersCount; i++)
        {
            var humanPlayer = Substitute.For<IPlayer>();
            humanPlayer.IsBot.Returns(false);
            humanPlayer.IsHuman.Returns(true);
            humanPlayers.Add(humanPlayer);
        }

        var players = botPlayers.Concat(humanPlayers).ToList();
        _game.Players.Returns(players);
        _sut.AttachHandlers();
        await Task.Delay(50);

        Assert.Equal(
            humanPlayersCount, 
            _apiClient.ReceivedCalls().Count());
    }

    [Fact]
    public void CloseButtonContent_HasValueReturnedByLocalizationService()
    {
        const string returnThis = "Close";
        _localizationService.GetLocalizedString("CloseButtonContent").Returns(returnThis);
        
        var result = _sut.CloseButtonContent;

        result.Should().Be(returnThis);
    }
    
    [Fact]
    public void Title_HasValueReturnedByLocalizationService()
    {
        const string returnThis = "Title";
        _localizationService.GetLocalizedString("GameFinishedLabel").Returns(returnThis);
        
        var result = _sut.Title;

        result.Should().Be(returnThis);
    }
    
    [Fact]
    public void AgainLabel_HasValueReturnedByLocalizationService()
    {
        const string returnThis = "Again";
        _localizationService.GetLocalizedString("AgainLabel").Returns(returnThis);
        
        var result = _sut.AgainLabel;

        result.Should().Be(returnThis);
    }
}