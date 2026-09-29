using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices.MVVM;
using NSubstitute;
using Sanet.Localization;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
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
    private readonly OnlineLobbyViewModel _sut;

    public OnlineLobbyViewModelTests()
    {
        _rules.GetAllRules().Returns(new[] { Rules.krSimple, Rules.krStandard });
        _sut = new OnlineLobbyViewModel(
            Substitute.For<IDicePanel>(),
            _localization,
            _rules,
            _players,
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

    private OnlineLobbyViewModel CreateViewModel(
        Func<IOnlineHostSession> hostSessionFactory,
        Func<IOnlineClientSession> clientSessionFactory) =>
        new(
            Substitute.For<IDicePanel>(),
            _localization,
            _rules,
            _players,
            hostSessionFactory,
            clientSessionFactory,
            _clipboard);
}
