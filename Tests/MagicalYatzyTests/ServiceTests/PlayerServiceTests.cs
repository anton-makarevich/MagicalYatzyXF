using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services;
using NSubstitute;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Sanet.MagicalYatzy.Extensions;

namespace MagicalYatzyTests.ServiceTests
{
    public class PlayerServiceTests
    {
        private IApiClient _apiMock;
        private IStorageService _storageMock;
        private PlayerService _sut;

        public PlayerServiceTests()
        {
            _apiMock = Substitute.For<IApiClient>();
            _storageMock = Substitute.For<IStorageService>();
            _sut = new PlayerService(_apiMock,_storageMock);
        }

        [Fact]
        public async Task PlayerServiceShouldHaveDefaultPlayer()
        {
            await _sut.LoadPlayersAsync();
            Assert.NotNull(_sut.CurrentPlayer);
        }

        [Fact]
        public async Task LoginShouldInsertPlayer()
        {
            // Arrange
            await _sut.LoadPlayersAsync();
            _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            // Act
            var result = await _sut.LoginAsync(TestUserName,TestUserPassword);
            // Asset
            Assert.True(result);
            Assert.True(_sut.Players.Count>1);
            Assert.Equal(_sut.CurrentPlayer, TestPlayer);
            Assert.Equal(_sut.Players[0], TestPlayer);
        }

        [Fact]
        public async Task SuccesfulLoginShouldSavePlayer()
        {
            // Arrange
            List<Player> players = new List<Player>();
            _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            _storageMock.SavePlayersAsync((List<Player>)_sut.Players).Returns((t) =>
            {
                players = (List<Player>)_sut.Players;
                return null;
            });

            // Act
            await _sut.LoginAsync(TestUserName, TestUserPassword);
            await Task.Delay(100);
            _storageMock.LoadPlayersAsync().Returns(Task.FromResult(players));
            _sut = new PlayerService(_apiMock, _storageMock);
            await _sut.LoadPlayersAsync();
            // Asset
            Assert.True(_sut.Players.Count == players.Count);
            for (int i = 0; i < players.Count;i++)
            {
                Assert.Equal(players[i], _sut.Players[i]);
            }
        }

        [Fact]
        public async Task FailingLoginShouldNotSavePlayer()
        {
            // Arrange
            _apiMock.LoginUserAsync(TestUserName, "1234").Returns(Task.FromResult<IPlayer>(null));


            // Act
            await _sut.LoginAsync(TestUserName, "1234");

            // Asset
            await _storageMock.DidNotReceiveWithAnyArgs().SavePlayersAsync(null);
        }

        [Fact]
        public async Task PlayersShouldNotContainMoreThanFourElements()
        {
            // Arrange
            List<Player> players = new List<Player>();
            List<string> passwords = new List<string>();

            for (int i = 0; i < 6; i++)
            {
                var username = $"Player{i}";
                var password = $"pw{i}";
                var player = new Player() { Name = username, Password = password };
                players.Add(player);
                passwords.Add(password);
                _apiMock.LoginUserAsync(username, password).Returns(Task.FromResult<IPlayer>(player));
            }

            // Act
            for (int i = 0; i < 6; i++)
            {
                await _sut.LoginAsync(players[i].Name, passwords[i]);
            }

            // Asset
            Assert.True(_sut.Players.Count == 4);
            Assert.Equal(players.Last(), _sut.Players.First());
            Assert.Equal(players[2], _sut.Players.Last());
        }

        [Fact]
        public async Task SameUserShouldNotBeAddedTwice()
        {
            // Arrange
            await _sut.LoadPlayersAsync();
            for (int i = 0; i < 6; i++)
            {
                _apiMock.LoginUserAsync(TestUserName, TestUserPassword).Returns(Task.FromResult(TestPlayer));
            }

            // Act
            for (int i = 0; i < 6; i++)
            {
                await _sut.LoginAsync(TestUserName, TestUserPassword);
            }

            // Asset
            Assert.True(_sut.Players.Count == 2);
        }

        [Fact]
        public async Task LoadPlayersShouldCallStorageOnlyOnce()
        {
            // Act
            await _sut.LoadPlayersAsync();
            await _sut.LoadPlayersAsync();

            // Asset
            Assert.Single(_storageMock.ReceivedCalls());
        }

        [Fact]
        public async Task LoadPlayersShouldTriggerUpdatePlayersOnlyOnce()
        {
            // Arrange
            int playersUpdatedCalledTimes = 0;
            _sut.PlayersUpdated += (s, e) => { playersUpdatedCalledTimes++; };

            // Act
            await _sut.LoadPlayersAsync();
            await _sut.LoadPlayersAsync();

            // Asset
            Assert.Equal(1, playersUpdatedCalledTimes);
        }

        [Fact]
        public async Task FaceboolLoginAddsPlayer()
        {
            // Arrange
            int playersUpdatedCalledTimes = 0;
            _sut.PlayersUpdated += (s, e) => { playersUpdatedCalledTimes++; };

            // Act
            await _sut.LoginToFacebookAsync();

            // Asset
            Assert.Equal(1, playersUpdatedCalledTimes);
        }

        public static string TestUserName => "Anton";
        public static string TestUserPassword => "123456";
        public static IPlayer TestPlayer { get; } = new Player { Name = TestUserName, Password = TestUserPassword.Encrypt(33) };
    }
}
