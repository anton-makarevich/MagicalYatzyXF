using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Sanet.MagicalYatzy.Extensions;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.StorageService;

public class LocalJsonStorageService : IStorageService
{
    public Task<List<IPlayer>> LoadPlayersAsync(string dataFile = null)
    {
        dataFile ??= DataFile;
        if (!File.Exists(dataFile))
            return Task.FromResult<List<IPlayer>>(null);
        return Task<List<IPlayer>>.Factory.StartNew(() =>
        {
            try
            {
                var stringData = File.ReadAllText(dataFile).Decrypt(32);
                return JsonSerializer.Deserialize<List<Player>>(stringData)
                    .Cast<IPlayer>().ToList();
            }
            catch
            {
                return null;
            }
        });
    }

    public Task SavePlayersAsync(List<IPlayer> players)
    {
        return Task.Factory.StartNew(() => 
        { 
            var stringData = JsonSerializer.Serialize(players).Encrypt(32);
            if (!Directory.Exists(DocumentsFolder))
                Directory.CreateDirectory(DocumentsFolder);
            File.WriteAllText(DataFile, stringData);
        });
    }

    private static string DocumentsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MagicalYatzy");
    private static readonly string DataFile = Path.Combine(DocumentsFolder, "players.data");
}
