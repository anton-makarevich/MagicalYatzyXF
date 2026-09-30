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
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;
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
    public void Browse_LoadsRulesAndSelectsSimpleByDefault()
    {
        _sut.State.ShouldBe(OnlineLobbyState.Browse);
        _sut.Rules.Count.ShouldBe(2);
        _sut.SelectedRule!.Rule.ShouldBe(Rules.krSimple);
    }

    [Fact]
    public void Browse_StartsWithNoRoomSelectedAndCannotJoin()
    {
        _sut.State.ShouldBe(OnlineLobbyState.Browse);
        _sut.SelectedRoom.ShouldBeNull();
        _sut.CanJoin.ShouldBeFalse();
    }

    [Fact]
    public void CurrentPlayerName_EditWritesThroughAndSurvivesModeSwitch()
    {
        var player = Substitute.For<IPlayer>();
        player.Name.Returns("Original");
        _players.CurrentPlayer.Returns(player);

        _sut.CurrentPlayerName = "Renamed";

        player.Name.ShouldBe("Renamed");

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

        WaitForRooms(sut, 0).GetAwaiter().GetResult();
        sut.SelectedRoom = new RoomViewModel(new RelayRoomInfo("ABC123", 1, Rules.krSimple), _localization);

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
    public async Task SelectingARoomOnlyMakesTheCodeAvailableForJoining()
    {
        _roomLister.ListRoomsAsync(Arg.Any<CancellationToken>()).Returns(RelayRoomListResult.Succeeded(
            [new RelayRoomInfo("JOIN12", 1, Rules.krSimple)]));
        var clientSession = Substitute.For<IOnlineClientSession>();
        var sut = CreateViewModel(
            () => Substitute.For<IOnlineHostSession>(),
            () => clientSession);
        sut.AttachHandlers();
        await WaitForRooms(sut, 1);

        sut.SelectedRoom = sut.Rooms.Single();

        sut.SelectedRoom.RoomCode.ShouldBe("JOIN12");
        sut.CanJoin.ShouldBeTrue();
        sut.State.ShouldBe(OnlineLobbyState.Browse);
        await clientSession.DidNotReceive().JoinAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
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
