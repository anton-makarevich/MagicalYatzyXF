using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sanet.MagicalYatzy.Online.Commands;
using Sanet.MagicalYatzy.Online.Commands.Client;
using Sanet.MagicalYatzy.Online.Commands.Server;
using Sanet.Transport;

namespace Sanet.MagicalYatzy.Online;

/// <summary>
/// The sole JSON (de)serialization point between online DTOs and the transport's
/// <see cref="TransportMessage"/> contract. Types are registered explicitly rather than discovered
/// by reflection so the mapping stays deterministic on trimmed/AOT mobile heads.
/// </summary>
public sealed class CommandRegistry
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new StringEnumConverter() }
    };

    private readonly Dictionary<string, Type> _typeByName = new();
    private readonly Dictionary<Type, string> _nameByType = new();

    public CommandRegistry()
    {
        RegisterAll();
    }

    /// <summary>
    /// All registered message types, keyed by their transport discriminator (CLR class name).
    /// </summary>
    public IReadOnlyDictionary<string, Type> Types => _typeByName;

    /// <summary>
    /// Looks up the CLR type for a transport discriminator, or <c>null</c> when unknown.
    /// </summary>
    public Type? GetType(string messageType) =>
        _typeByName.TryGetValue(messageType, out var type) ? type : null;

    /// <summary>
    /// Looks up the transport discriminator for a CLR type, or <c>null</c> when unregistered.
    /// </summary>
    public string? GetName(Type type) =>
        _nameByType.TryGetValue(type, out var name) ? name : null;

    /// <summary>
    /// Converts an online message to a transport message. The transport discriminator is the
    /// message's CLR class name; <see cref="OnlineMessage.MessageType"/> is not serialized into the
    /// payload.
    /// </summary>
    public TransportMessage ToTransportMessage(OnlineMessage message, Guid sourceId)
    {
        var payload = JsonConvert.SerializeObject(message, message.GetType(), SerializerSettings);
        return new TransportMessage
        {
            MessageType = message.MessageType,
            SourceId = sourceId,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Deserializes a transport message back to an online message. Returns <c>null</c> (rather than
    /// throwing) for an unknown discriminator or an invalid payload.
    /// </summary>
    public OnlineMessage? TryDeserialize(TransportMessage message)
    {
        var type = GetType(message.MessageType);
        if (type == null)
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject(message.Payload, type, SerializerSettings) as OnlineMessage;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void RegisterAll()
    {
        Register<ReadyCommand>();
        Register<RollCommand>();
        Register<FixDiceCommand>();
        Register<FixAllDiceCommand>();
        Register<MagicRollCommand>();
        Register<ResetRollsCommand>();
        Register<ManualChangeCommand>();
        Register<ApplyScoreCommand>();
        Register<ChangeStyleCommand>();
        Register<LeaveGameCommand>();
        Register<RequestGameStateCommand>();

        Register<PlayerJoinedBroadcast>();
        Register<PlayerLeftBroadcast>();
        Register<PlayerReadyBroadcast>();
        Register<TurnChangedBroadcast>();
        Register<DiceRolledBroadcast>();
        Register<DiceFixedBroadcast>();
        Register<DiceChangedBroadcast>();
        Register<PlayerRerolledBroadcast>();
        Register<MagicRollUsedBroadcast>();
        Register<StyleChangedBroadcast>();
        Register<ScoreAppliedBroadcast>();
        Register<GameStateBroadcast>();
        Register<GameFinishedBroadcast>();
        Register<GameRestartedBroadcast>();
        Register<GameEndedBroadcast>();
    }

    private void Register<T>() where T : OnlineMessage
    {
        var name = typeof(T).Name;
        if (_typeByName.ContainsKey(name))
        {
            throw new InvalidOperationException($"Online message type '{name}' is already registered.");
        }

        _typeByName[name] = typeof(T);
        _nameByType[typeof(T)] = name;
    }
}
