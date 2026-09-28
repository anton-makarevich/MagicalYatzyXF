using System;
using System.Collections.Generic;
using System.Linq;
using Sanet.MagicalYatzy.Models.Chat;
using Sanet.MagicalYatzy.Models.Events;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Models.Game.Magical;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.MagicalYatzy.Online.Commands.Server.State;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Client-side <see cref="IGame"/> projection of an online game hosted elsewhere: a value-backed
/// mirror of the authoritative <see cref="YatzyServerGame"/>. Hydrates from <see cref="GameState"/>
/// snapshots and changes state only when a host broadcast arrives (re-raising the matching
/// <see cref="IGame"/> event afterwards). Local calls are converted into intent commands for the
/// host - the host alone changes authoritative state. Only turn actions of the local player and
/// local-player readiness/style/leave intents are sent; host-owned operations are no-ops.
/// </summary>
public sealed class ClientYatzyGame : IGame
{
    private readonly string _localPlayerId;
    private readonly Action<OnlineMessage> _sendCommand;

    private List<IPlayer> _players = [];
    private List<int> _fixedRollResults = [];
    private int[] _lastDiceValues = [];

    // Set while a MagicRollUsedBroadcast precedes its matching DiceRolledBroadcast, so the
    // roll-count increment (which the host skips for magic rolls) is skipped here as well.
    private bool _magicRollPending;

    private bool _isFinished;

    public ClientYatzyGame(string localPlayerId, Action<OnlineMessage> sendCommand)
    {
        _localPlayerId = localPlayerId;
        _sendCommand = sendCommand;
    }

    #region Events

    /// <summary>Raised when the projection is (re)hydrated from a snapshot.</summary>
    public event EventHandler? GameUpdated;

    public event EventHandler<PlayerEventArgs>? PlayerLeft;
    public event EventHandler<RollEventArgs>? DiceChanged;
    public event EventHandler<FixDiceEventArgs>? DiceFixed;
    public event EventHandler<RollEventArgs>? DiceRolled;
    public event EventHandler<PlayerEventArgs>? PlayerReady;
    public event EventHandler<PlayerEventArgs>? PlayerRerolled;
    public event EventHandler<PlayerEventArgs>? MagicRollUsed;
    public event EventHandler<PlayerEventArgs>? StyleChanged;
    public event EventHandler? GameFinished;
    public event EventHandler<MoveEventArgs>? TurnChanged;
    public event EventHandler<ChatMessageEventArgs>? ChatMessageSent;
    public event EventHandler<PlayerEventArgs>? PlayerJoined;
    public event EventHandler<RollResultEventArgs>? ResultApplied;

    #endregion

    #region Properties

    public IPlayer? CurrentPlayer { get; private set; }

    public string GameId { get; private set; } = string.Empty;

    public Rule Rules { get; private set; } = null!;

    public int Round { get; private set; }

    public bool IsPlaying { get; set; }

    /// <summary>Stores the value only - the host owns the roll-mode side effects.</summary>
    public bool ReRollMode { get; set; }

    public List<IPlayer> Players
    {
        get => _players;
        private set => _players = value;
    }

    public int NumberOfPlayers => _players.Count;

    public int NumberOfFixedDice => _fixedRollResults.Count;

    /// <summary>A copy of the currently fixed dice face values, duplicates preserved.</summary>
    public IReadOnlyList<int> FixedRollResults => _fixedRollResults.ToList();

    public DieResult LastDiceResult => new()
    {
        DiceResults = _lastDiceValues.ToList()
    };

    public int Roll
    {
        get
        {
            var roll = 1;
            if (CurrentPlayer != null && CurrentPlayer.Roll > roll)
                roll = CurrentPlayer.Roll;

            return (roll <= YatzyGame.MaxRoll) ? roll : YatzyGame.MaxRoll;
        }
    }

    #endregion

    #region Snapshot hydration

    /// <summary>
    /// Hydrates the projection from a full <see cref="GameState"/> snapshot. Existing players are
    /// restored in place (identity preserved for subscribers); new seats are created. Raises only
    /// <see cref="GameUpdated"/> - no gameplay events are replayed for a snapshot.
    /// </summary>
    public void Hydrate(GameState state)
    {
        GameId = state.GameId;
        Rules = new Rule(state.Rule);
        Round = state.Round;
        IsPlaying = state.IsPlaying;
        ReRollMode = state.ReRollMode;
        _lastDiceValues = state.LastDiceValues.ToArray();
        _fixedRollResults = state.FixedDiceValues.ToList();

        var restored = new List<IPlayer>();
        foreach (var playerState in state.Players)
        {
            var player = _players.FirstOrDefault(p => p.InGameId == playerState.PlayerId);
            if (player == null)
            {
                player = new Player(MapSeatType(playerState), playerState.Name);
            }

            RestorePlayer(player, playerState, playerState.PlayerId == state.CurrentPlayerId);
            restored.Add(player);
        }

        _players = restored;
        CurrentPlayer = FindPlayer(state.CurrentPlayerId);

        ClearPossibleValues();
        // The host refreshes possible values for the current player's own rolls only; a turn
        // without a roll keeps zero possible values (values from an earlier turn were dropped).
        if (CurrentPlayer != null && CurrentPlayer.Roll >= 2 && _lastDiceValues.Length > 0)
        {
            CurrentPlayer.CheckRollResults(LastDiceResult, Rules);
        }

        GameUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void RestorePlayer(IPlayer player, PlayerState state, bool isCurrentPlayer)
    {
        ((Player)player).RestoreFromSnapshot(
            state.PlayerId,
            Rules,
            state.Scores.Select(s => (s.ScoreType, s.Value, s.HasValue, s.HasBonus)),
            state.Artifacts.Select(a => (a.Type, a.IsUsed)));
        player.Name = state.Name;
        player.SeatNo = state.SeatNo;
        player.IsReady = state.IsReady;
        player.Roll = state.Roll;
        player.SelectedStyle = state.SelectedStyle;
        player.IsMyTurn = isCurrentPlayer;
    }

    /// <summary>
    /// The local guest seat is <see cref="PlayerType.Local"/>; every other human seat is network
    /// from this client's perspective (the host's own local seat included), AI seats stay AI.
    /// </summary>
    private PlayerType MapSeatType(PlayerState state) =>
        state.PlayerId == _localPlayerId
            ? PlayerType.Local
            : state.Type == PlayerType.AI
                ? PlayerType.AI
                : PlayerType.Network;

    #endregion

    #region Broadcast application

    /// <summary>
    /// Single entry point for host broadcasts: updates projected state first, then re-raises the
    /// matching <see cref="IGame"/> event. Broadcasts for unknown players are ignored. Session-level
    /// broadcasts (game ended/restarted) are not handled here.
    /// </summary>
    public void ApplyBroadcast(OnlineMessage message)
    {
        if (message is null || Rules is null)
        {
            return;
        }

        switch (message)
        {
            case PlayerJoinedBroadcast joined:
                ApplyPlayerJoined(joined);
                break;
            case PlayerLeftBroadcast left:
                ApplyPlayerLeft(left);
                break;
            case PlayerReadyBroadcast ready:
                ApplyPlayerReady(ready);
                break;
            case StyleChangedBroadcast style:
                ApplyStyleChanged(style);
                break;
            case TurnChangedBroadcast turn:
                ApplyTurnChanged(turn);
                break;
            case DiceRolledBroadcast rolled:
                ApplyDiceRolled(rolled);
                break;
            case DiceChangedBroadcast changed:
                ApplyDiceChanged(changed);
                break;
            case DiceFixedBroadcast fixedDie:
                ApplyDiceFixed(fixedDie);
                break;
            case PlayerRerolledBroadcast rerolled:
                ApplyPlayerRerolled(rerolled);
                break;
            case MagicRollUsedBroadcast magicRoll:
                ApplyMagicRollUsed(magicRoll);
                break;
            case ScoreAppliedBroadcast score:
                ApplyScoreApplied(score);
                break;
            case GameFinishedBroadcast finished:
                ApplyGameFinished(finished);
                break;
            case GameStateBroadcast snapshot:
                Hydrate(snapshot.State);
                break;
        }
    }

    private void ApplyPlayerJoined(PlayerJoinedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            player = new Player(MapRemoteType(broadcast.Type), broadcast.Name);
            ((Player)player).RestoreFromSnapshot(broadcast.PlayerId, Rules, [], []);
            player.SeatNo = broadcast.SeatNo;
            player.IsMyTurn = false;
            player.IsReady = false;
            player.Roll = 1;
            _players.Add(player);
        }

        PlayerJoined?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyPlayerLeft(PlayerLeftBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        _players.Remove(player);
        PlayerLeft?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyPlayerReady(PlayerReadyBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        player.IsReady = broadcast.IsReady;
        PlayerReady?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyStyleChanged(StyleChangedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        player.SelectedStyle = broadcast.Style;
        StyleChanged?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyTurnChanged(TurnChangedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        Round = broadcast.Round;
        IsPlaying = true;
        _fixedRollResults = [];
        _magicRollPending = false;
        ClearPossibleValues();
        foreach (var seatedPlayer in _players)
        {
            seatedPlayer.IsMyTurn = false;
        }

        CurrentPlayer = player;
        player.IsMyTurn = true;

        TurnChanged?.Invoke(this, new MoveEventArgs(player, Round));
    }

    private void ApplyDiceRolled(DiceRolledBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        _lastDiceValues = broadcast.Values.ToArray();
        player.CheckRollResults(LastDiceResult, Rules);
        DiceRolled?.Invoke(this, new RollEventArgs(player, [.. _lastDiceValues]));

        if (_magicRollPending)
        {
            // the host does not consume a roll for a magic roll - the paired
            // MagicRollUsedBroadcast precedes this broadcast
            _magicRollPending = false;
            return;
        }

        player.Roll++;
    }

    private void ApplyDiceChanged(DiceChangedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        _lastDiceValues = broadcast.Values.ToArray();
        player.CheckRollResults(LastDiceResult, Rules);
        DiceChanged?.Invoke(this, new RollEventArgs(player, [.. _lastDiceValues]));
    }

    private void ApplyDiceFixed(DiceFixedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        // The host always sends All = false and emits one broadcast per fixed/unfixed die,
        // so a single multiset occurrence changes per broadcast.
        if (broadcast.IsFixed)
        {
            _fixedRollResults.Add(broadcast.Value);
        }
        else
        {
            _fixedRollResults.Remove(broadcast.Value);
        }

        DiceFixed?.Invoke(this, new FixDiceEventArgs(player, broadcast.Value, broadcast.IsFixed));
    }

    private void ApplyPlayerRerolled(PlayerRerolledBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        // Local consequences of YatzyGame.ResetRolls: roll counter back to one, roll mode on,
        // fixed dice cleared (the store-only ReRollMode setter has no side effects here).
        player.Roll = 1;
        ReRollMode = true;
        _fixedRollResults = [];
        PlayerRerolled?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyMagicRollUsed(MagicRollUsedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        _lastDiceValues = broadcast.Values.ToArray();
        player.CheckRollResults(LastDiceResult, Rules);
        player.UseArtifact(Artifacts.MagicalRoll);
        _magicRollPending = true;
        MagicRollUsed?.Invoke(this, new PlayerEventArgs(player));
    }

    private void ApplyScoreApplied(ScoreAppliedBroadcast broadcast)
    {
        var player = FindPlayer(broadcast.PlayerId);
        if (player == null)
        {
            return;
        }

        // Commit to the sheet first (as YatzyServerGame does), so subscribers of ResultApplied
        // observe already-committed state. Every broadcast is applied - the host sends a separate
        // broadcast for an auto-applied numeric bonus.
        var result = player.GetResultForScore(broadcast.ScoreType);
        if (result == null)
        {
            return;
        }

        result.Value = broadcast.Value;
        result.HasBonus = broadcast.HasBonus;
        ResultApplied?.Invoke(this,
            new RollResultEventArgs(player, broadcast.Value, broadcast.ScoreType, broadcast.HasBonus));
    }

    private void ApplyGameFinished(GameFinishedBroadcast broadcast)
    {
        if (_isFinished)
        {
            return;
        }

        _isFinished = true;
        var ordered = broadcast.Standings
            .Select(standing => FindPlayer(standing.PlayerId))
            .Where(p => p != null)
            .Cast<IPlayer>()
            .ToList();
        if (ordered.Count > 0)
        {
            _players = ordered;
        }

        CurrentPlayer = _players.FirstOrDefault();
        IsPlaying = false;
        foreach (var player in _players)
        {
            player.IsReady = false;
        }

        GameFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ends the game locally (host left or connection lost). Raises <see cref="GameFinished"/>
    /// once, only if the game has not already finished.
    /// </summary>
    public void EndGameLocally()
    {
        if (_isFinished)
        {
            return;
        }

        _isFinished = true;
        IsPlaying = false;
        GameFinished?.Invoke(this, EventArgs.Empty);
    }

    private PlayerType MapRemoteType(PlayerType type) =>
        type == PlayerType.Local ? PlayerType.Network : type;

    private IPlayer? FindPlayer(string? playerId) =>
        _players.FirstOrDefault(p => p.InGameId == playerId);

    private void ClearPossibleValues()
    {
        foreach (var player in _players)
        {
            if (player.Results == null)
            {
                continue;
            }

            foreach (var result in player.Results)
            {
                if (!result.HasValue)
                {
                    result.PossibleValue = 0;
                }
            }
        }
    }

    #endregion

    #region Local intents

    public void ReportRoll()
    {
        SendTurnIntent(new RollCommand { PlayerId = _localPlayerId });
    }

    public void ReportMagicRoll()
    {
        SendTurnIntent(new MagicRollCommand { PlayerId = _localPlayerId });
    }

    public void ResetRolls()
    {
        SendTurnIntent(new ResetRollsCommand { PlayerId = _localPlayerId });
    }

    public void FixDice(int value, bool isfixed)
    {
        SendTurnIntent(new FixDiceCommand { PlayerId = _localPlayerId, Value = value, IsFixed = isfixed });
    }

    public void FixAllDice(int value, bool isFixed)
    {
        SendTurnIntent(new FixAllDiceCommand { PlayerId = _localPlayerId, Value = value, IsFixed = isFixed });
    }

    public void ManualChange(int oldValue, int newValue, bool isFixed)
    {
        SendTurnIntent(new ManualChangeCommand
        {
            PlayerId = _localPlayerId,
            OldValue = oldValue,
            NewValue = newValue,
            IsFixed = isFixed
        });
    }

    public void ApplyScore(IRollResult result)
    {
        SendTurnIntent(new ApplyScoreCommand { PlayerId = _localPlayerId, ScoreType = result.ScoreType });
    }

    public void SetPlayerReady(IPlayer player, bool isReady)
    {
        if (!IsLocalPlayer(player))
        {
            return;
        }

        _sendCommand(new ReadyCommand { PlayerId = _localPlayerId, IsReady = isReady });
    }

    public void ChangeStyle(IPlayer player, DiceStyle style)
    {
        if (!IsLocalPlayer(player))
        {
            return;
        }

        _sendCommand(new ChangeStyleCommand { PlayerId = _localPlayerId, Style = style });
    }

    public void LeaveGame(IPlayer player)
    {
        if (!IsLocalPlayer(player))
        {
            return;
        }

        _sendCommand(new LeaveGameCommand { PlayerId = _localPlayerId });
    }

    // Host-owned operations - no-ops on the projection side.
    public void JoinGame(IPlayer player)
    {
    }

    public void DoTurn()
    {
    }

    public void NextTurn()
    {
    }

    public void RestartGame()
    {
    }

    public void SendChatMessage(ChatMessage message)
    {
        // the host does not bridge chat - ChatMessageSent is never raised
    }

    public bool IsDiceFixed(int value)
    {
        return _fixedRollResults.Contains(value);
    }

    private void SendTurnIntent(OnlineMessage command)
    {
        // Turn actions are sent only while the local player holds the turn; the host
        // re-validates everything against its own authoritative state anyway.
        if (!IsLocalPlayer(CurrentPlayer))
        {
            return;
        }

        _sendCommand(command);
    }

    private bool IsLocalPlayer(IPlayer? player) =>
        player != null && player.InGameId == _localPlayerId;

    #endregion
}
