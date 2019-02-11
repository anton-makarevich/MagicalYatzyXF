using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;

namespace Sanet.MagicalYatzy.Services.Game
{
    public interface IRulesService
    {
        IEnumerable<Rules> GetAllRules();
    }
}