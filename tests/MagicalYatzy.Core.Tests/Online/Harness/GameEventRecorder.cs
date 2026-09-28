using System.Threading;
using Sanet.MagicalYatzy.Online;

namespace MagicalYatzy.Core.Tests.Online.Harness;

/// <summary>
/// Counts the projection events a guest session raises, so online tests can assert how many
/// rolls, scores, turns and game ends a client actually observed. Hydration replays no events,
/// so a recorder has to be attached right after the seat snapshot is applied.
/// </summary>
public sealed class GameEventRecorder
{
    public int TurnChanged;
    public int DiceRolled;
    public int ResultApplied;
    public int GameFinished;

    public static GameEventRecorder Attach(ClientYatzyGame game)
    {
        var recorder = new GameEventRecorder();
        game.TurnChanged += (_, _) => Interlocked.Increment(ref recorder.TurnChanged);
        game.DiceRolled += (_, _) => Interlocked.Increment(ref recorder.DiceRolled);
        game.ResultApplied += (_, _) => Interlocked.Increment(ref recorder.ResultApplied);
        game.GameFinished += (_, _) => Interlocked.Increment(ref recorder.GameFinished);
        return recorder;
    }
}
