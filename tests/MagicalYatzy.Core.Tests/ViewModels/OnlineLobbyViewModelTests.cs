using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using NSubstitute;
using Sanet.Localization;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MagicalYatzy.ViewModels;
using Shouldly;
using Xunit;

namespace MagicalYatzy.Core.Tests.ViewModels;

public sealed class OnlineLobbyViewModelTests
{
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();
    private readonly IRulesService _rules = Substitute.For<IRulesService>();
    private readonly IPlayerService _players = Substitute.For<IPlayerService>();
    private readonly IClipboardService _clipboard = Substitute.For<IClipboardService>();
    private readonly IRelayRoomLister _roomLister = Substitute.For<IRelayRoomLister>();
    private readonly OnlineLobbyViewModel _sut;

    public OnlineLobbyViewModelTests()
    {
        _rules.GetAllRules().Returns(new[] { Rules.krSimple, Rules.krStandard });
        _localization.GetString("RoomPlayersFormat").Returns("{0}/{1} players");
        _localization.GetString("krMagic").Returns("Magic");
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded([]));
        _sut = new OnlineLobbyViewModel(
            Substitute.For<IDicePanel>(),
            _localization,
            _rules,
            _players,
            _roomLister,
            Substitute.For<Func<IOnlineHostSession>>(),
            Substitute.For<Func<IOnlineClientSession>>(),
            _clipboard);
    }

    [Fact]
    public void HostMode_LoadsRulesAndSelectsSimpleByDefault()
    {
        _sut.SelectHostCommand.Execute(null);

        _sut.State.ShouldBe(OnlineLobbyState.HostSetup);
        _sut.Rules.Count.ShouldBe(2);
        _sut.SelectedRule!.Rule.ShouldBe(Rules.krSimple);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc12")]
    [InlineData("abc1234")]
    [InlineData("abc-12")]
    public void JoinCode_InvalidValues_CannotJoin(string code)
    {
        _sut.JoinCode = code;

        _sut.CanJoin.ShouldBeFalse();
    }

    [Fact]
    public void JoinCode_TrimmedSixCharacterValue_CanJoinWithoutChangingCase()
    {
        _sut.JoinCode = " Abc123 ";

        _sut.CanJoin.ShouldBeTrue();
        _sut.JoinCode.ShouldBe(" Abc123 ");
    }

    [Fact]
    public void JoinMode_LoadsRulesButKeepsThemReadOnly()
    {
        _sut.SelectJoinCommand.Execute(null);

        _sut.State.ShouldBe(OnlineLobbyState.JoinSetup);
        _sut.Rules.Count.ShouldBe(2);
        _sut.IsRulesEditable.ShouldBeFalse();

        _sut.SelectHostCommand.Execute(null);

        _sut.IsRulesEditable.ShouldBeTrue();
    }

    [Fact]
    public void CurrentPlayerName_EditWritesThroughAndSurvivesModeSwitch()
    {
        var player = Substitute.For<IPlayer>();
        player.Name.Returns("Original");
        _players.CurrentPlayer.Returns(player);

        _sut.CurrentPlayerName = "Renamed";

        player.Name.ShouldBe("Renamed");

        _sut.SelectHostCommand.Execute(null);
        _sut.SelectJoinCommand.Execute(null);

        _sut.CurrentPlayerName.ShouldBe("Renamed");
        player.Name.ShouldBe("Renamed");
    }

    [Fact]
    public async Task ReattachedViewModel_HostsWithANonCancelledToken()
    {
        var hostSession = Substitute.For<IOnlineHostSession>();
        CancellationToken? hostToken = null;
        hostSession
            .HostAsync(Arg.Any<Rules>(), Arg.Do<CancellationToken>(token => hostToken = token))
            .Returns(OnlineHostResult.Succeeded("ABC123"));
        var sut = CreateViewModel(() => hostSession, () => Substitute.For<IOnlineClientSession>());

        sut.AttachHandlers();
        sut.DetachHandlers();
        sut.AttachHandlers();

        await ((IAsyncCommand) sut.CreateRoomCommand).ExecuteAsync();

        hostToken.ShouldNotBeNull();
        hostToken.Value.IsCancellationRequested.ShouldBeFalse();
    }

    [Fact]
    public async Task ReattachedViewModel_JoinsWithANonCancelledToken()
    {
        var clientSession = Substitute.For<IOnlineClientSession>();
        CancellationToken? joinToken = null;
        clientSession
            .JoinAsync(Arg.Any<string>(), Arg.Do<CancellationToken>(token => joinToken = token))
            .Returns(OnlineClientResult.Succeeded("ABC123"));
        var sut = CreateViewModel(() => Substitute.For<IOnlineHostSession>(), () => clientSession);

        sut.AttachHandlers();
        sut.DetachHandlers();
        sut.AttachHandlers();
        sut.JoinCode = "ABC123";

        await ((IAsyncCommand) sut.JoinCommand).ExecuteAsync();

        joinToken.ShouldNotBeNull();
        joinToken.Value.IsCancellationRequested.ShouldBeFalse();
    }

    [Fact]
    public async Task DetachedViewModel_CancelsTheHostingToken()
    {
        var hostSession = Substitute.For<IOnlineHostSession>();
        CancellationToken? hostToken = null;
        hostSession
            .HostAsync(Arg.Any<Rules>(), Arg.Do<CancellationToken>(token => hostToken = token))
            .Returns(OnlineHostResult.Succeeded("ABC123"));
        var sut = CreateViewModel(() => hostSession, () => Substitute.For<IOnlineClientSession>());

        sut.AttachHandlers();
        await ((IAsyncCommand) sut.CreateRoomCommand).ExecuteAsync();

        sut.DetachHandlers();

        hostToken.ShouldNotBeNull();
        hostToken.Value.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task AttachingLoadsRoomsEvenBeforeJoinModeIsPicked()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("ABC123", 1, Rules.krMagic)]));

        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => Substitute.For<IOnlineClientSession>());
        sut.AttachHandlers();

        await WaitForRooms(sut, 1);

        sut.Rooms.Single().RoomCode.ShouldBe("ABC123");
        sut.Rooms.Single().PlayersText.ShouldBe("1/4 players");
        sut.Rooms.Single().RulesText.ShouldBe("Magic");
    }

    [Fact]
    public async Task TogglingIntoJoinModeRefreshesTheRoomList()
    {
        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => Substitute.For<IOnlineClientSession>());
        sut.AttachHandlers();
        await WaitForRooms(sut, 0);

        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("XYZ789", 2, Rules.krSimple)]));

        sut.SelectJoinCommand.Execute(null);

        await WaitForRooms(sut, 1);
        sut.Rooms.Single().RoomCode.ShouldBe("XYZ789");
    }

    [Fact]
    public async Task RefreshingReplacesThePreviousListing()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("AAA111", 1, Rules.krSimple),
             new RelayRoomInfo("BBB222", 2, Rules.krStandard)]));
        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => Substitute.For<IOnlineClientSession>());
        sut.AttachHandlers();
        await WaitForRooms(sut, 2);

        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("CCC333", 3, Rules.krExtended)]));
        await ((IAsyncCommand) sut.RefreshRoomsCommand).ExecuteAsync();

        await WaitForRooms(sut, 1);
        sut.Rooms.Single().RoomCode.ShouldBe("CCC333");
    }

    [Fact]
    public async Task FullRoomsAreNotJoinable()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("FULL01", 4, Rules.krSimple)]));
        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => Substitute.For<IOnlineClientSession>());
        sut.AttachHandlers();

        await WaitForRooms(sut, 1);

        sut.Rooms.Single().CanJoin.ShouldBeFalse();
    }

    [Fact]
    public async Task FailedListingShowsTheErrorAndClearsStaleRooms()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("AAA111", 1, Rules.krSimple)]));
        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => Substitute.For<IOnlineClientSession>());
        sut.AttachHandlers();
        await WaitForRooms(sut, 1);

        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Failed("hub down"));
        await ((IAsyncCommand) sut.RefreshRoomsCommand).ExecuteAsync();

        await WaitForRooms(sut, 0);
        sut.RoomsErrorMessage.ShouldBe("hub down");
        sut.HasRoomsError.ShouldBeTrue();
        sut.IsRoomsLoading.ShouldBeFalse();
    }

    [Fact]
    public async Task SelectingARoomJoinsItWithTheRoomsCode()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("JOIN12", 1, Rules.krSimple)]));
        var clientSession = Substitute.For<IOnlineClientSession>();
        clientSession
            .JoinAsync("JOIN12", Arg.Any<CancellationToken>())
            .Returns(OnlineClientResult.Succeeded("JOIN12"));
        var sut = new OnlineLobbyViewModel(
            Substitute.For<IDicePanel>(),
            _localization,
            _rules,
            _players,
            _roomLister,
            Substitute.For<Func<IOnlineHostSession>>(),
            () => clientSession,
            _clipboard);
        sut.AttachHandlers();
        await WaitForRooms(sut, 1);

        sut.Rooms.Single().JoinCommand.Execute(null);

        await clientSession.Received(1).JoinAsync("JOIN12", Arg.Any<CancellationToken>());
    }

    private static async Task WaitForRooms(OnlineLobbyViewModel sut, int expected)
    {
        var limit = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while (sut.Rooms.Count != expected && DateTime.UtcNow < limit)
            await Task.Delay(10);

        sut.Rooms.Count.ShouldBe(expected);
    }

    private OnlineLobbyViewModel CreateViewModel(
        Func<IOnlineHostSession> hostSessionFactory,
        Func<IOnlineClientSession> clientSessionFactory) =>
        new(
            Substitute.For<IDicePanel>(),
            _localization,
            _rules,
            _players,
            _roomLister,
            hostSessionFactory,
            clientSessionFactory,
            _clipboard);
}
