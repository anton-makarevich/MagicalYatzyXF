using Sanet.MagicalYatzy.Models.Game;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Sanet.MagicalYatzy.Services
{
    public interface IPlayerService 
    {
        IReadOnlyList<IPlayer> Players { get; }

        IPlayer CurrentPlayer { get; }

        Task<bool> LoginToFacebookAsync();

        Task<bool> LoginAsync(string newUsername, string newPassword);

        event EventHandler PlayersUpdated;
    }
}
