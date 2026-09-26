using System.Text.Json.Serialization;

namespace Sanet.MagicalYatzy.Online.Commands;

/// <summary>
/// Base type for every online message. <see cref="MessageType"/> is the CLR class name and is used
/// by the command registry as the transport discriminator; it is deliberately not serialized into
/// the JSON payload.
/// </summary>
public abstract class OnlineMessage
{
    [JsonIgnore]
    public string MessageType => GetType().Name;

    /// <summary>
    /// The acting or affected player's in-game id. May be empty for game-level broadcasts.
    /// </summary>
    public string PlayerId { get; init; } = string.Empty;
}
