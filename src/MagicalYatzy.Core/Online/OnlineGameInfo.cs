using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.Transport.Relay.Contracts;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// Identity this app reports to the relay hub and uses to discover its own rooms. The same
/// <see cref="GameId"/> is sent when hosting (<see cref="Create"/>) and when listing
/// (<see cref="OnlineGameInfo.Filter"/>), so a room only shows up for the client that can play it.
/// </summary>
public static class OnlineGameInfo
{
    /// <summary>
    /// Game title identifier shared by every MagicalYatzy room. Must stay within
    /// <see cref="RoomGameInfoLimits"/> so it is safe for hub-side query-string filtering.
    /// </summary>
    public const string GameId = "magical-yatzy";

    /// <summary>
    /// <see cref="RoomGameInfo.Metadata"/> key carrying the room rule, valued as
    /// <see cref="Rules"/> name (for example <c>"krMagic"</c>).
    /// </summary>
    public const string RulesMetadataKey = "rules";

    /// <summary>Seat count of an online game; the host fills the first seat.</summary>
    public const int MaxPlayers = 4;

    /// <summary>Filter selecting every open MagicalYatzy room, whatever rule it was created with.</summary>
    public static RoomListFilter Filter => new(GameId);

    /// <summary>
    /// Game version reported at room creation, trimmed to <see cref="RoomGameInfoLimits.MaxVersionLength"/>
    /// so it always satisfies the hub's validation. Falls back to the assembly version when no
    /// informational version is stamped.
    /// </summary>
    public static string Version { get; } = ResolveVersion();

    /// <summary>
    /// Builds the <see cref="RoomGameInfo"/> a host reports for its room: the host's game instance
    /// id, the game title, this app's version and the room rule as metadata.
    /// </summary>
    public static RoomGameInfo Create(Guid hostId, Rules rule) =>
        new(hostId,
            GameId,
            Version,
            new Dictionary<string, string>(StringComparer.Ordinal) { [RulesMetadataKey] = rule.ToString() });

    /// <summary>
    /// Reads the room rule back from a listed room's metadata. Returns <c>false</c> when the room
    /// carries no rule, an unknown rule name, or was reported by a build that no longer has it.
    /// Parsing ignores case, so a room survives an older build that cased a name differently.
    /// </summary>
    public static bool TryGetRule(IReadOnlyDictionary<string, string>? metadata, out Rules rule)
    {
        rule = default;
        if (metadata == null
            || !metadata.TryGetValue(RulesMetadataKey, out var value)
            || !Enum.TryParse(value, true, out rule)
            || !Enum.IsDefined(rule))
        {
            rule = default;
            return false;
        }

        return true;
    }

    private static string ResolveVersion()
    {
        var informational = typeof(OnlineGameInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        // Strip the "+<commit sha>" suffix the SDK appends, then fall back to the assembly version.
        var plusIndex = informational?.IndexOf('+') ?? -1;
        var version = plusIndex > 0 ? informational![..plusIndex] : informational;

        if (string.IsNullOrWhiteSpace(version))
        {
            version = typeof(OnlineGameInfo).Assembly.GetName().Version?.ToString();
        }

        version = version?.Trim();
        if (string.IsNullOrEmpty(version))
        {
            Debug.Assert(false, "MagicalYatzy version could not be resolved; reporting '0.0.0' to the hub.");
            return "0.0.0";
        }

        return version.Length > RoomGameInfoLimits.MaxVersionLength
            ? version[..RoomGameInfoLimits.MaxVersionLength]
            : version;
    }
}