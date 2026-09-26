using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Online.Commands.Client;

/// <summary>
/// Client intent to apply a score to the given category. The server computes the value from its
/// own authoritative state.
/// </summary>
public sealed class ApplyScoreCommand : OnlineMessage
{
    public Scores ScoreType { get; init; }
}
