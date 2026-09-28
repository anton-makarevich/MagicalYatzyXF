using System;
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
    public void JoinMode_DoesNotLoadHostRules()
    {
        _sut.SelectJoinCommand.Execute(null);

        _sut.State.ShouldBe(OnlineLobbyState.JoinSetup);
        _sut.Rules.ShouldBeEmpty();
    }
}