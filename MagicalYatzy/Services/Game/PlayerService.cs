﻿using Sanet.MagicalYatzy.Models.Game;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Sanet.MagicalYatzy.Services.Api;
using Sanet.MagicalYatzy.Services.StorageService;

namespace Sanet.MagicalYatzy.Services.Game
{
    public class PlayerService: IPlayerService
    {
        private readonly IApiClient _apiClient;
        private readonly IStorageService _storageService;

        private List<Player> _players = new List<Player> ();

        public event EventHandler PlayersUpdated;

        public PlayerService(IApiClient apiClient, IStorageService storageService)
        {
            _apiClient = apiClient;
            _storageService = storageService;
        }

        public async Task LoadPlayersAsync()
        {
            if (Players.Any())
                return;
            var players = await _storageService.LoadPlayersAsync();
            if (players != null && players.Any())
                _players = players;
            else
                _players.Add(new Player());
            PlayersUpdated?.Invoke(this, null);
        }

        private int PlayerToBeSetIndex { get; set; }

        public IReadOnlyList<IPlayer> Players => _players;

        public IPlayer CurrentPlayer => (!Players.Any())? null: Players[PlayerToBeSetIndex]; 

        public async Task<IPlayer> LoginAsync(string username, string password)
        {
            var player = await _apiClient.LoginUserAsync(username, password);
            if (player == null)
                return null;
            AddPlayer((Player)player);
            return player;
        }

        public async Task<IPlayer> LoginToFacebookAsync()
        {
            var player = await Task.FromResult(new Player());
            AddPlayer(player);
            return player; 
        }

        private void AddPlayer(Player player)
        {
            var oldPlayer = _players.FirstOrDefault(p => p.Equals(player));
            if (oldPlayer != null)
                _players.Remove(oldPlayer);
            _players.Insert(0,player);
            if (_players.Count > 4)
                _players.RemoveAt(_players.Count - 1);
            _storageService.SavePlayersAsync(_players);
            PlayerToBeSetIndex = _players.IndexOf(player);
            PlayersUpdated?.Invoke(this, null);
        }
    }
}
