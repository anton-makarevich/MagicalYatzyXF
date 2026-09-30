using System;
using Sanet.Transport.Relay.Contracts;

namespace MagicalYatzy.Core.Tests.Online.Fakes;

/// <summary>
/// Game info the relay stub echoes back on create/join, standing in for a real host's report.
/// </summary>
internal static class TestGameInfo
{
    public static RoomGameInfo Create(Guid? hostId = null) =>
        new(hostId ?? Guid.NewGuid(), "magical-yatzy", "1.0.0");
}
