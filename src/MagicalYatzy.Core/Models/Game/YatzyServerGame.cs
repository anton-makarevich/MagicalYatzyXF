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
/// as no ViewModel does it here.
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

    public override void ApplyScore(IRollResult result)
    {
        lock (_syncRoot)
        {
            base.ApplyScore(result);
        }
    }

    protected override void CommitResult(IRollResult result, int value, bool hasBonus)
    {
        var target = CurrentPlayer?.Results?.FirstOrDefault(f => f.ScoreType == result.ScoreType) ?? result;
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