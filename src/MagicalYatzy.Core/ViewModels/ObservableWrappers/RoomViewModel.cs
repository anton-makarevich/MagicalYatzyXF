using System;
using System.Windows.Input;
using Sanet.Localization;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

/// <summary>
/// A listed relay room offered for joining: its code, how full it is and the rule its host
/// reported. <see cref="JoinCommand"/> raises <see cref="RoomSelected"/>; the owning view model
/// decides whether to actually join.
/// </summary>
public class RoomViewModel : BindableBase
{
    private readonly ILocalizationService _localizationService;

    public RoomViewModel(RelayRoomInfo room, ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        RoomCode = room.RoomCode;
        MemberCount = room.MemberCount;
        Rule = room.Rule;
    }

    public event EventHandler? RoomSelected;

    /// <summary>Code used to join the room.</summary>
    public string RoomCode { get; }

    /// <summary>Device sessions currently in the room; the host counts as one.</summary>
    public int MemberCount { get; }

    /// <summary>The room's rule, or <c>null</c> when the host reported none this build knows.</summary>
    public Rules? Rule { get; }

    /// <summary>Seated and seat count, for example <c>"1/4"</c>.</summary>
    public string PlayersText => string.Format(
        _localizationService.GetString("RoomPlayersFormat"), MemberCount, OnlineGameInfo.MaxPlayers);

    /// <summary>Localized rule name, or a placeholder for rooms reporting an unknown rule.</summary>
    public string RulesText => Rule is { } rule
        ? _localizationService.GetString(rule.ToString())
        : _localizationService.GetString("UnknownRulesLabel");

    /// <summary>A full room cannot be joined, so its item is not clickable.</summary>
    public bool CanJoin => MemberCount < OnlineGameInfo.MaxPlayers;

    public ICommand JoinCommand => new SimpleCommand(() =>
    {
        if (CanJoin)
            RoomSelected?.Invoke(this, EventArgs.Empty);
    });
}