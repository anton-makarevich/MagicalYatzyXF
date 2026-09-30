using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.Localization;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Online;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Services;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Relay;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels;

public enum OnlineLobbyState
{
    ChooseMode,
    HostSetup,
    Creating,
    Hosting,
    JoinSetup,
    Joining,
    Joined,
    Started,
    Failed
}

public sealed class OnlineLobbyViewModel : DicePanelViewModel
{
    private static readonly Regex RoomCodePattern = new("^[A-Za-z0-9]{6}$", RegexOptions.Compiled);

    private readonly ILocalizationService _localizationService;
    private readonly IRulesService _rulesService;
    private readonly IPlayerService _playerService;
    private readonly IRelayRoomLister _roomLister;
    private readonly Func<IOnlineHostSession> _hostSessionFactory;
    private readonly Func<IOnlineClientSession> _clientSessionFactory;
    private readonly IClipboardService _clipboardService;
    private CancellationTokenSource _lifetimeCancellation = new();

    private IOnlineHostSession? _hostSession;
    private IOnlineClientSession? _clientSession;
    private SynchronizationContext? _synchronizationContext;
    private string? _failureMessage;
    private bool _isCreateMode = true;

    public OnlineLobbyViewModel(
        IDicePanel dicePanel,
        ILocalizationService localizationService,
        IRulesService rulesService,
        IPlayerService playerService,
        IRelayRoomLister roomLister,
        Func<IOnlineHostSession> hostSessionFactory,
        Func<IOnlineClientSession> clientSessionFactory,
        IClipboardService clipboardService) : base(dicePanel)
    {
        _localizationService = localizationService;
        _rulesService = rulesService;
        _playerService = playerService;
        _roomLister = roomLister;
        _hostSessionFactory = hostSessionFactory;
        _clientSessionFactory = clientSessionFactory;
        _clipboardService = clipboardService;

        SelectHostCommand = new SimpleCommand(() => SetMode(true));
        SelectJoinCommand = new SimpleCommand(() => SetMode(false));
        CreateRoomCommand = new AsyncCommand(CreateRoomAsync);
        CopyCodeCommand = new AsyncCommand(() => _clipboardService.SetTextAsync(RoomCode ?? string.Empty));
        RefreshRoomsCommand = new AsyncCommand(() => RefreshRoomsAsync(_lifetimeCancellation.Token));
        JoinCommand = new AsyncCommand(JoinAsync);
        ReadyCommand = new SimpleCommand(Ready);
        StartGameCommand = new SimpleCommand(StartGame);
        AddBotCommand = new SimpleCommand(() => { });
        AddHumanCommand = new SimpleCommand(() => { });
    }

    public ObservableCollection<RuleViewModel> Rules { get; } = new();

    public ObservableCollection<PlayerViewModel> Players { get; } = new();

    /// <summary>Open rooms reported by the relay hub, offered for joining.</summary>
    public ObservableCollection<RoomViewModel> Rooms { get; } = new();

    public ICommand SelectHostCommand { get; }
    public ICommand SelectJoinCommand { get; }
    public ICommand CreateRoomCommand { get; }
    public ICommand CopyCodeCommand { get; }
    public ICommand RefreshRoomsCommand { get; }
    public ICommand JoinCommand { get; }
    public ICommand ReadyCommand { get; }
    public ICommand StartGameCommand { get; }
    public ICommand AddBotCommand { get; }
    public ICommand AddHumanCommand { get; }

    public OnlineLobbyState State
    {
        get;
        private set => SetProperty(ref field, value);
    } = OnlineLobbyState.ChooseMode;

    public string Title => _localizationService.GetString("NewOnlineGameAction");
    public string PlayersTitle => _localizationService.GetString("PlayersLabel");
    public string CurrentPlayerName
    {
        get => _playerService.CurrentPlayer?.Name;
        set
        {
            var player = _playerService.CurrentPlayer;
            if (player == null || player.Name == value)
                return;
            player.Name = value;
            NotifyPropertyChanged();
        }
    }
    public string CurrentPlayerTypeName => _playerService.CurrentPlayer?.Type == PlayerType.AI
        ? _localizationService.GetString("BotNameDefault")
        : _localizationService.GetString("PlayerNameDefault");
    public string CurrentPlayerImage => string.IsNullOrEmpty(_playerService.CurrentPlayer?.ProfileImage)
        ? "SanetDice.png"
        : _playerService.CurrentPlayer.ProfileImage;
    public string HostGameLabel => _localizationService.GetString("HostGameLabel");
    public string JoinGameLabel => _localizationService.GetString("JoinGameLabel");
    public string RulesTitle => _localizationService.GetString("RulesLabel").ToUpper();
    public string CreateRoomLabel => _localizationService.GetString("CreateRoomLabel");
    public string RoomCodeLabel => _localizationService.GetString("RoomCodeLabel");
    public string CopyCodeLabel => _localizationService.GetString("CopyCodeLabel");
    public string RoomsTitle => _localizationService.GetString("RoomsLabel");
    public string RefreshRoomsLabel => _localizationService.GetString("RefreshRoomsLabel");
    public string NoRoomsMessage => _localizationService.GetString("NoRoomsMessage");
    public string ReadyLabel => _localizationService.GetString("ReadyLabel");
    public string StartLabel => _localizationService.GetString("StartGameButton");
    public string StartImage => "Start.png";
    public string BackImage => "Back.png";

    public bool IsCreateMode
    {
        get => _isCreateMode;
        set => SetMode(value);
    }

    public bool IsModeSelectionVisible => State is OnlineLobbyState.ChooseMode
        or OnlineLobbyState.HostSetup
        or OnlineLobbyState.JoinSetup
        or OnlineLobbyState.Failed;

    public string? RoomCode { get; private set; }

    /// <summary>The room currently picked in the room list, or <c>null</c> when nothing is picked.</summary>
    public RoomViewModel? SelectedRoom
    {
        get;
        set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(CanJoin));
        }
    }

    public RuleViewModel SelectedRule
    {
        get => Rules.FirstOrDefault(rule => rule.IsSelected);
        set
        {
            foreach (var rule in Rules)
                rule.IsSelected = rule == value;
            NotifyPropertyChanged();
        }
    }

    public bool CanJoin => State == OnlineLobbyState.JoinSetup && SelectedRoom?.CanJoin == true;

    /// <summary>
    /// The room list is offered whenever no session is running yet, so it is already populated
    /// on page load and stays available while toggling between hosting and joining.
    /// </summary>
    public bool IsRoomsVisible => State is OnlineLobbyState.ChooseMode
        or OnlineLobbyState.HostSetup
        or OnlineLobbyState.JoinSetup
        or OnlineLobbyState.Failed;

    public bool IsRoomsLoading
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(HasNoRoomsMessage));
        }
    }

    /// <summary>
    /// Reason the room list could not be loaded, or <c>null</c> when the last load succeeded.
    /// Shown in place of the list; a stale list is cleared so a failure never shows rooms as
    /// if they were current.
    /// </summary>
    public string? RoomsErrorMessage
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(HasRoomsError));
        }
    }

    public bool HasRoomsError => !string.IsNullOrEmpty(RoomsErrorMessage);

    public bool HasRooms => Rooms.Count > 0;

    public bool HasNoRoomsMessage => IsRoomsVisible && !IsRoomsLoading && !HasRoomsError && Rooms.Count == 0;

    public bool CanCreate => State == OnlineLobbyState.HostSetup;

    public bool CanAddBot => false;

    public bool CanAddHuman => false;

    public string AddBotLabel => string.Empty;

    public string AddPlayerLabel => string.Empty;

    public string AddBotImage => string.Empty;

    public string AddPlayerImage => string.Empty;

    public bool IsRulesEditable => State == OnlineLobbyState.HostSetup;

    public bool CanCopyCode => State == OnlineLobbyState.Hosting && !string.IsNullOrEmpty(RoomCode);

    public bool CanStartGame => State == OnlineLobbyState.Hosting
                                && _hostSession?.Game is { Players.Count: >= 2 } game
                                && game.Players.Where(player => player.InGameId != _hostSession.HostPlayer?.InGameId)
                                    .All(player => player.IsReady);

    public bool CanReady => State == OnlineLobbyState.Joined
                            && _clientSession?.LocalPlayer is { IsReady: false };

    public bool IsChooseMode => State == OnlineLobbyState.ChooseMode;
    public bool IsHostSetup => State == OnlineLobbyState.HostSetup;
    public bool IsCreating => State == OnlineLobbyState.Creating;
    public bool IsHosting => State == OnlineLobbyState.Hosting;
    public bool IsJoinSetup => State == OnlineLobbyState.JoinSetup;
    public bool IsJoining => State == OnlineLobbyState.Joining;
    public bool IsJoined => State == OnlineLobbyState.Joined;
    public bool IsStarted => State == OnlineLobbyState.Started;
    public bool IsFailed => State == OnlineLobbyState.Failed;

    public string StatusMessage => _failureMessage
                                   ?? State switch
                                   {
                                       OnlineLobbyState.Hosting => _localizationService.GetString(
                                           "WaitingForPlayersMessage"),
                                       OnlineLobbyState.Joined => _clientSession?.Game?.IsPlaying == true
                                           ? _localizationService.GetString("GameStartedMessage")
                                           : _localizationService.GetString("WaitingForHostMessage"),
                                       OnlineLobbyState.Started => _localizationService.GetString("GameStartedMessage"),
                                       OnlineLobbyState.Failed =>
                                           _localizationService.GetString("ServerOfflineMessage"),
                                       _ => _localizationService.GetString("NetworkGameNotReadyMessage")
                                   };

    public override void AttachHandlers()
    {
        base.AttachHandlers();
        _synchronizationContext = SynchronizationContext.Current;
        _lifetimeCancellation = new CancellationTokenSource();
        SetMode(true);
        RefreshRooms();
    }

    public override void DetachHandlers()
    {
        _lifetimeCancellation.Cancel();
        _lifetimeCancellation.Dispose();
        UnsubscribeFromGame();
        _ = DisposeSessionAsync();
        base.DetachHandlers();
    }

    private void ChangeState(OnlineLobbyState state)
    {
        State = state;
        if (state != OnlineLobbyState.Failed)
            _failureMessage = null;
        if (state is OnlineLobbyState.HostSetup or OnlineLobbyState.JoinSetup)
            LoadRules();
        NotifyStateChanged();
    }

    private void SetMode(bool createMode)
    {
        var modeChanged = _isCreateMode != createMode;
        if (modeChanged)
        {
            _isCreateMode = createMode;
            NotifyPropertyChanged(nameof(IsCreateMode));
        }

        var state = createMode ? OnlineLobbyState.HostSetup : OnlineLobbyState.JoinSetup;
        var stateChanged = State != state;
        ChangeState(state);

        // Every join-mode activation refreshes the list, so a room list opened by toggling the
        // switch never shows a snapshot taken before the previous join session.
        if (!createMode && (modeChanged || stateChanged))
            RefreshRooms();
    }

    /// <summary>
    /// Fire-and-forget room-list refresh; safe to call before the view model is attached, where
    /// <see cref="_lifetimeCancellation"/> is still the field initializer's token.
    /// </summary>
    private void RefreshRooms() => _ = RefreshRoomsAsync(_lifetimeCancellation.Token);

    private async Task RefreshRoomsAsync(CancellationToken cancellationToken)
    {
        IsRoomsLoading = true;

        RelayRoomListResult? result = null;
        try
        {
            result = await _roomLister.ListRoomsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // The lobby is going away; the current list is left alone.
        }
        catch (Exception exception)
        {
            var message = string.IsNullOrWhiteSpace(exception.Message)
                ? _localizationService.GetString("RoomsUnavailableMessage")
                : exception.Message;
            result = RelayRoomListResult.Failed(message);
        }

        var outcome = result;
        PostToContext(() => ApplyRooms(outcome));
    }

    /// <summary>
    /// Swaps the visible list for the latest listing. Marshalled to the attaching thread because
    /// <see cref="Rooms"/> is bound; a failed listing empties the list so stale rooms are never
    /// offered as if they were current.
    /// </summary>
    private void ApplyRooms(RelayRoomListResult? result)
    {
        // A null result means the listing was cancelled because the lobby went away; the last
        // known list is deliberately left in place rather than being wiped.
        if (result == null)
        {
            IsRoomsLoading = false;
            return;
        }

        RoomsErrorMessage = result.Success
            ? null
            : result.Error ?? _localizationService.GetString("RoomsUnavailableMessage");

        RebuildRooms(result.Rooms);

        IsRoomsLoading = false;
        NotifyPropertyChanged(nameof(HasRooms));
        NotifyPropertyChanged(nameof(HasNoRoomsMessage));
    }

    private void RebuildRooms(IReadOnlyList<RelayRoomInfo> rooms)
    {
        Rooms.Clear();
        foreach (var room in rooms)
            Rooms.Add(new RoomViewModel(room, _localizationService));
    }

    private void LoadRules()
    {
        if (Rules.Count > 0)
            return;
        foreach (var rule in _rulesService.GetAllRules().Select(rule => new RuleViewModel(rule, _localizationService)))
        {
            rule.RuleSelected += OnRuleSelected;
            Rules.Add(rule);
        }

        SelectedRule = Rules.FirstOrDefault(rule => rule.Rule == Sanet.MagicalYatzy.Models.Game.Rules.krSimple);
    }

    private void OnRuleSelected(object? sender, EventArgs e)
    {
        SelectedRule = sender as RuleViewModel;
    }

    private async Task CreateRoomAsync()
    {
        if (SelectedRule == null)
            return;
        await DisposeSessionAsync();
        _failureMessage = null;
        ChangeState(OnlineLobbyState.Creating);
        var session = _hostSessionFactory();
        _hostSession = session;
        try
        {
            var result = await session.HostAsync(SelectedRule.Rule, _lifetimeCancellation.Token);
            if (!result.Success || string.IsNullOrWhiteSpace(result.RoomCode) || session.Game == null)
            {
                await FailSessionAsync(session, result.Error);
                return;
            }

            RoomCode = result.RoomCode;
            SubscribeToGame(session.Game);
            RebuildPlayers(session.Game);
            ChangeState(OnlineLobbyState.Hosting);
            NotifyPropertyChanged(nameof(RoomCode));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailSessionAsync(session, exception.Message);
        }
    }

    private async Task JoinAsync()
    {
        var roomCode = SelectedRoom?.RoomCode;
        if (string.IsNullOrEmpty(roomCode) || !RoomCodePattern.IsMatch(roomCode))
        {
            NotifyStateChanged();
            return;
        }

        await DisposeSessionAsync();
        _failureMessage = null;
        ChangeState(OnlineLobbyState.Joining);
        var session = _clientSessionFactory();
        _clientSession = session;
        try
        {
            var result = await session.JoinAsync(roomCode, _lifetimeCancellation.Token);
            if (!result.Success || session.Game == null)
            {
                await FailSessionAsync(session, result.Error);
                return;
            }

            RoomCode = result.RoomCode ?? roomCode;
            SubscribeToGame(session.Game);
            RebuildPlayers(session.Game);
            SelectedRule = Rules.FirstOrDefault(rule => rule.Rule == session.Game.Rules.CurrentRule);
            ChangeState(OnlineLobbyState.Joined);
            NotifyPropertyChanged(nameof(RoomCode));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailSessionAsync(session, exception.Message);
        }
    }

    private void StartGame()
    {
        if (!CanStartGame || _hostSession?.HostPlayer == null)
            return;
        _hostSession.SubmitLocalCommand(new ReadyCommand
        {
            PlayerId = _hostSession.HostPlayer.InGameId,
            IsReady = true
        });
    }

    private void Ready()
    {
        if (!CanReady || _clientSession?.Game == null || _clientSession.LocalPlayer == null)
            return;
        _clientSession.Game.SetPlayerReady(_clientSession.LocalPlayer, true);
    }

    private void SubscribeToGame(IGame game)
    {
        game.PlayerJoined += OnGameChanged;
        game.PlayerLeft += OnGameChanged;
        game.PlayerReady += OnGameChanged;
        game.TurnChanged += OnTurnChanged;
        game.GameFinished += OnGameChanged;
        if (_clientSession != null)
            _clientSession.GameEnded += OnGameEnded;
    }

    private void UnsubscribeFromGame()
    {
        if (_hostSession?.Game != null)
            UnsubscribeFromGame(_hostSession.Game);
        if (_clientSession?.Game != null)
            UnsubscribeFromGame(_clientSession.Game);
        if (_clientSession != null)
            _clientSession.GameEnded -= OnGameEnded;
    }

    private void UnsubscribeFromGame(IGame game)
    {
        game.PlayerJoined -= OnGameChanged;
        game.PlayerLeft -= OnGameChanged;
        game.PlayerReady -= OnGameChanged;
        game.TurnChanged -= OnTurnChanged;
        game.GameFinished -= OnGameChanged;
    }

    private void OnGameChanged(object? sender, EventArgs e) => PostToContext(() =>
    {
        if (sender is IGame game)
            RebuildPlayers(game);
        else if (_hostSession?.Game != null)
            RebuildPlayers(_hostSession.Game);
        else if (_clientSession?.Game != null)
            RebuildPlayers(_clientSession.Game);
        CheckStarted();
    });

    private void OnTurnChanged(object? sender, Sanet.MagicalYatzy.Models.Events.MoveEventArgs e) =>
        PostToContext(CheckStarted);

    private void OnGameEnded(GameEndReason reason) => PostToContext(() =>
    {
        _failureMessage = _localizationService.GetString("ServerOfflineMessage");
        ChangeState(OnlineLobbyState.Failed);
    });

    private void CheckStarted()
    {
        var game = (IGame?)_hostSession?.Game ?? _clientSession?.Game;
        if (game?.IsPlaying == true)
            ChangeState(OnlineLobbyState.Started);
        else
            NotifyStateChanged();
    }

    private void RebuildPlayers(IGame game)
    {
        var localPlayerId = _clientSession?.LocalPlayer?.InGameId ?? _hostSession?.HostPlayer?.InGameId;
        var hostPlayerId = _hostSession?.HostPlayer?.InGameId ?? game.Players.FirstOrDefault()?.InGameId;
        Players.Clear();
        foreach (var player in game.Players)
        {
            var playerViewModel = new PlayerViewModel(player, _localizationService)
            {
                CanBeDeleted = false
            };
            Players.Add(playerViewModel);
        }

        NotifyStateChanged();
    }

    private async Task FailSessionAsync(IAsyncDisposable session, string? error)
    {
        _failureMessage = error;
        await session.DisposeAsync();
        if (ReferenceEquals(session, _hostSession))
            _hostSession = null;
        if (ReferenceEquals(session, _clientSession))
            _clientSession = null;
        RoomCode = null;
        ChangeState(OnlineLobbyState.Failed);
        NotifyPropertyChanged(nameof(RoomCode));
    }

    private async Task DisposeSessionAsync()
    {
        UnsubscribeFromGame();
        if (_clientSession?.Game != null && _clientSession.LocalPlayer != null)
            _clientSession.Game.LeaveGame(_clientSession.LocalPlayer);
        var host = _hostSession;
        var client = _clientSession;
        _hostSession = null;
        _clientSession = null;
        if (host != null)
            await host.DisposeAsync();
        if (client != null)
            await client.DisposeAsync();
    }

    private void PostToContext(Action action)
    {
        if (_synchronizationContext == null)
            action();
        else
            _synchronizationContext.Post(_ => action(), null);
    }

    private void NotifyStateChanged()
    {
        NotifyPropertyChanged(nameof(IsChooseMode));
        NotifyPropertyChanged(nameof(IsHostSetup));
        NotifyPropertyChanged(nameof(IsCreating));
        NotifyPropertyChanged(nameof(IsHosting));
        NotifyPropertyChanged(nameof(IsJoinSetup));
        NotifyPropertyChanged(nameof(IsJoining));
        NotifyPropertyChanged(nameof(IsJoined));
        NotifyPropertyChanged(nameof(IsStarted));
        NotifyPropertyChanged(nameof(IsFailed));
        NotifyPropertyChanged(nameof(IsRoomsVisible));
        NotifyPropertyChanged(nameof(HasNoRoomsMessage));
        NotifyPropertyChanged(nameof(StatusMessage));
        NotifyPropertyChanged(nameof(CanCreate));
        NotifyPropertyChanged(nameof(CanJoin));
        NotifyPropertyChanged(nameof(CanCopyCode));
        NotifyPropertyChanged(nameof(CanStartGame));
        NotifyPropertyChanged(nameof(CanReady));
        NotifyPropertyChanged(nameof(IsRulesEditable));
        NotifyPropertyChanged(nameof(IsModeSelectionVisible));
        NotifyPropertyChanged(nameof(CurrentPlayerName));
        NotifyPropertyChanged(nameof(CurrentPlayerTypeName));
        NotifyPropertyChanged(nameof(CurrentPlayerImage));
    }
}
