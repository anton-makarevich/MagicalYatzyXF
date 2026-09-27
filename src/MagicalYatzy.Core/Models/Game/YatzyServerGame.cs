using System;
using System.Linq;
using System.Threading;
using Sanet.MagicalYatzy.Models.Game.DiceGenerator;

namespace Sanet.MagicalYatzy.Models.Game;

/// <summary>
/// Server-side <see cref="YatzyGame"/>: commits applied results itself
/// (no attached ViewModel does it), keeps restart ready-gating and enforces
/// a round timeout that auto-fills the current player's first open non-Bonus
/// score to keep the game moving.
/// </summary>
/// <remarks>
/// Round-timeout scoring runs on a thread-pool thread, so the game's events
/// (ResultApplied, TurnChanged, GameFinished, ...) can be raised off the
/// caller's thread; subscribers must marshal to their own context as needed.
/// A server host must also refresh <see cref="IRollResult.PossibleValue"/>
/// for the current roll (e.g. via <see cref="IPlayer.CheckRollResults"/>)
/// as no ViewModel does it here. Values left from an earlier turn are dropped
/// when a turn starts, so a turn without a roll is auto-filled with zero.
/// Scores that are not on the current player's sheet, or that are already
/// filled, are rejected.
/// </remarks>
public class YatzyServerGame : YatzyGame, IDisposable
{
    /// <summary>
    /// Round timeout after which the current player's first open non-Bonus
    /// score is auto-filled with its current value. Non-positive disables the
    /// timeout. Default: 100 seconds.
    /// </summary>
    public TimeSpan RoundTimeout { get; set; } = TimeSpan.FromSeconds(100);

    private Timer? _roundTimer;
    private int _turnGeneration;

    public YatzyServerGame()
    {
    }

    public YatzyServerGame(Rules rules, IDiceGenerator diceGenerator) : base(rules, diceGenerator)
    {
    }

    public override void DoTurn()
    {
        ClearPossibleValues();
        base.DoTurn();
    }

    public override void ApplyScore(IRollResult result)
    {
        lock (_syncRoot)
        {
            //a score that is not on the current player's sheet, or that is already
            //filled, can never be committed, so applying it would advance the turn
            //without changing state
            var target = FindSheetResult(result);
            if (target == null || target.HasValue)
                return;

            base.ApplyScore(result);
        }
    }

    protected override void CommitResult(IRollResult result, int value, bool hasBonus)
    {
        var target = FindSheetResult(result);
        if (target == null)
            return;
        target.Value = value;
        target.HasBonus = hasBonus;
    }

    protected override void SetPlayerReadyForRestart(IPlayer player)
    {
        // no auto-ready: server players must report readiness again after restart
    }

    protected override void StartTurnTimer()
    {
        lock (_syncRoot)
        {
            _turnGeneration++;
            _roundTimer?.Dispose();
            if (RoundTimeout <= TimeSpan.Zero)
                return;
            var generation = _turnGeneration;
            _roundTimer = new Timer(OnRoundTimeout, generation, RoundTimeout, Timeout.InfiniteTimeSpan);
        }
    }

    protected override void StopTurnTimer()
    {
        lock (_syncRoot)
        {
            _turnGeneration++;
            _roundTimer?.Dispose();
            _roundTimer = null;
        }
    }

    private void OnRoundTimeout(object? state)
    {
        var generation = (int)state!;
        lock (_syncRoot)
        {
            if (generation != _turnGeneration || !IsPlaying)
                return;
            var player = CurrentPlayer;
            if (player == null || !player.IsMyTurn || !player.IsReady)
                return;
            var openResult = player.Results?
                .FirstOrDefault(f => f.ScoreType != Scores.Bonus && !f.HasValue);
            if (openResult != null)
                ApplyScore(openResult);
        }
    }

    /// <summary>
    /// Drops values computed for a previous turn, so a turn without a roll
    /// can only be auto-filled with zero. A server host refreshes values of the
    /// current roll itself (see <see cref="IPlayer.CheckRollResults"/>).
    /// </summary>
    private void ClearPossibleValues()
    {
        foreach (var player in Players)
        {
            if (player.Results == null)
                continue;
            foreach (var result in player.Results)
            {
                if (!result.HasValue)
                    result.PossibleValue = 0;
            }
        }
    }

    private IRollResult? FindSheetResult(IRollResult? result)
    {
        return result == null
            ? null
            : CurrentPlayer?.Results?.FirstOrDefault(f => f.ScoreType == result.ScoreType);
    }

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            _turnGeneration++;
            _roundTimer?.Dispose();
            _roundTimer = null;
        }
    }
}