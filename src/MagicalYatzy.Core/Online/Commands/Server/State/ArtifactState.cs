using Sanet.MagicalYatzy.Models.Game.Magical;

namespace Sanet.MagicalYatzy.Online.Commands.Server.State;

/// <summary>
/// Value-only snapshot of an artifact and whether it was used.
/// </summary>
public sealed class ArtifactState
{
    public Artifacts Type { get; init; }

    public bool IsUsed { get; init; }
}
