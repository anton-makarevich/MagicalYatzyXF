using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.Services.StorageService;
using Xunit;

namespace MagicalYatzyTests.Services.Game
{
    public class PlayerServiceTests
    {
        private readonly IApiClient _apiMock;
        private readonly IStorageService _storageMock;
        private PlayerService _sut;
        private readonly ILocalizationService _localizationServiceMock;

        public PlayerServiceTests()
        {
            _apiMock = Substitute.For<IApiClient>();
            _storageMock = Substitute.For<IStorageService>();
            _localizationServiceMock = Substitute.For<ILocalizationService>();
            _sut = new PlayerService(_apiMock,_storageMock, _localizationServiceMock);
        }

        [Fact]
        public async Task PlayerServiceHasDefaultPlayer()
        {
            await _sut.LoadPlayersAsync();
            Assert.NotNull(_sut.CurrentPlayer);
        }

        [Fact]
        public async Task LoginInsertsPlayer()
        {
            // Arrange
            await _sut.LoadPlayersAsync();
            _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            // Act
            var result = await _sut.LoginAsync(TestUserName,TestUserPassword);
            // Asset
            Assert.Equal(TestPlayer,result);
            Assert.True(_sut.Players.Count>1);
            Assert.Equal(TestPlayer,_sut.CurrentPlayer);
            Assert.Equal(TestPlayer, _sut.Players[0]);
        }

        [Fact]
        public async Task SuccessfullyLogsInSavesPlayer()
        {
            // Arrange
            var players = new List<IPlayer>();
            _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            _storageMock.SavePlayersAsync((List<IPlayer>)_sut.Players).Returns((t) =>
            {
                players = (List<IPlayer>)_sut.Players;
                return null;
            });

            // Act
            await _sut.LoginAsync(TestUserName, TestUserPassword);
            await Task.Delay(100);
            _storageMock.LoadPlayersAsync().Returns(Task.FromResult(players));
            _sut = new PlayerService(_apiMock, _storageMock,_localizationServiceMock);
            await _sut.LoadPlayersAsync();
            // Asset
            Assert.True(_sut.Players.Count == players.Count);
            for (var i = 0; i < players.Count;i++)
            {
                Assert.Equal(players[i], _sut.Players[i]);
            }
        }

        [Fact]
        public async Task FailingLoginDoesNotSavePlayer()
        {
            // Arrange
            _apiMock.LoginUserAsync(TestUserName, "1234").Returns(Task.FromResult<IPlayer>(null));


            // Act
            await _sut.LoginAsync(TestUserName, "1234");

            // Asset
            await _storageMock.DidNotReceiveWithAnyArgs().SavePlayersAsync(null);
        }

        [Fact]
        public async Task PlayersListDoesNotContainMoreThanFourElements()
        {
            // Arrange
            var players = new List<Player>();
            var passwords = new List<string>();

            for (var i = 0; i < 6; i++)
            {
                var username = $"Player{i}";
                var password = $"pw{i}";
                var player = new Player() { Name = username, Password = password };
                players.Add(player);
                passwords.Add(password);
                _apiMock.LoginUserAsync(username, password).Returns(Task.FromResult<IPlayer>(player));
            }

            // Act
            for (var i = 0; i < 6; i++)
            {
                await _sut.LoginAsync(players[i].Name, passwords[i]);
            }

            // Asset
            Assert.True(_sut.Players.Count == 4);
            Assert.Equal(players.Last(), _sut.Players.First());
            Assert.Equal(players[2], _sut.Players.Last());
        }

        [Fact]
        public async Task SameUserIsNotAddedTwice()
        {
            // Arrange
            await _sut.LoadPlayersAsync();
            for (var i = 0; i < 6; i++)
            {
                _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            }

            // Act
            for (var i = 0; i < 6; i++)
            {
                await _sut.LoginAsync(TestUserName, TestUserPassword);
            }

            // Asset
            Assert.True(_sut.Players.Count == 2);
        }

        [Fact]
        public async Task LoadPlayersCallsStorageOnlyOnce()
        {
            // Act
            await _sut.LoadPlayersAsync();
            await _sut.LoadPlayersAsync();

            // Asset
            await _storageMock.Received(1).LoadPlayersAsync();
        }

        [Fact]
        public async Task LoadPlayersTriggersUpdatePlayersOnlyOnce()
        {
            // Arrange
            var playersUpdatedCalledTimes = 0;
            _sut.PlayersUpdated += (s, e) =>
            {
                playersUpdatedCalledTimes++;
            };

            // Act
            await _sut.LoadPlayersAsync();
            await _sut.LoadPlayersAsync();

            // Asset
            Assert.Equal(1, playersUpdatedCalledTimes);
        }

        [Fact]
        public async Task FacebookLoginAddsPlayer()
        {
            // Arrange
            var playersUpdatedCalledTimes = 0;
            _sut.PlayersUpdated += (s, e) => { playersUpdatedCalledTimes++; };

            // Act
            await _sut.LoginToFacebookAsync();

            // Asset
            Assert.Equal(1, playersUpdatedCalledTimes);
        }
        
        [Fact]
        public void PlayerHasDefaultNameIfThereAreMoreThanTenPlayersWithIncreasingDefNames()
        {
            var existedPlayersNames = new List<string>();
            for (var index = 0; index < 11; index++)
            {
                existedPlayersNames.Add($"Bot {index}");
            }
            
            var player = _sut.CreateLocalPlayer("Bot",PlayerType.AI, existedPlayersNames);
            
            Assert.Equal("Bot 1", player.Name);
        }
        
        [Fact]
        public void NextAddedBotHasIncreasedNumber()
        {
            var existedPlayersNames = new List<string>(){"Bot 1"};
            
            var player = _sut.CreateLocalPlayer("Bot",PlayerType.AI, existedPlayersNames);
            
            Assert.Equal("Bot 2", player.Name);
        }

        public static string TestUserName => "Anton";
        public static string TestUserPassword => "123456";
        private static IPlayer TestPlayer { get; } =
            new Player {Name = TestUserName, Password = TestUserPassword.Encrypt(33)};
    }
}
