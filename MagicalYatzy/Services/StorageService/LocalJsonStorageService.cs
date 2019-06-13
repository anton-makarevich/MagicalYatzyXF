using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.StorageService
{
    public class LocalJsonStorageService : IStorageService
    {
        public Task<List<Player>> LoadPlayersAsync(string dataFile = null)
        {
            if (dataFile == null)
                dataFile = DataFile;
            if (!File.Exists(dataFile))
                return Task.FromResult<List<Player>>(null);
            return Task<List<Player>>.Factory.StartNew(() =>
            {
                try
                {
                    var stringData = File.ReadAllText(dataFile).Decrypt(32);
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
