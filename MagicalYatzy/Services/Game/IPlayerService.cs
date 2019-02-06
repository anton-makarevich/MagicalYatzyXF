using Sanet.MagicalYatzy.Models.Game;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Sanet.MagicalYatzy.Services.Game
{
    public interface IPlayerService 
    {
        IReadOnlyList<IPlayer> Players { get; }

        IPlayer CurrentPlayer { get; }

        Task<IPlayer> LoginToFacebookAsync();

        Task<IPlayer> LoginAsync(string newUsername, string newPassword);

        Task LoadPlayersAsync();

        event EventHandler PlayersUpdated;
    }
}
