using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Models.Game;
using Newtonsoft.Json;
using System.IO;
using Sanet.MagicalYatzy.Extensions;

namespace Sanet.MagicalYatzy.Services
{
    public class LocalJsonStorageService : IStorageService
    {
        public Task<List<Player>> LoadPlayersAsync()
        {
            if (!File.Exists(DataFile))
                return Task.FromResult<List<Player>>(null);
            return Task<List<Player>>.Factory.StartNew(() =>
            {
                try
                {
                    var stringData = File.ReadAllText(DataFile).Decrypt(32);
                    return JsonConvert.DeserializeObject<List<Player>>(stringData);
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task SavePlayersAsync(List<Player> players)
        {
            return Task.Factory.StartNew(() => 
            { 
                var stringData = JsonConvert.SerializeObject(players).Encrypt(32);
                if (!Directory.Exists(DocumentsFolder))
                    Directory.CreateDirectory(DocumentsFolder);
                File.WriteAllText(DataFile, stringData);
            });
        }

        private static string DocumentsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MagicalYatzy");
        private static readonly string DataFile = Path.Combine(DocumentsFolder, "players.data");
    }
}
