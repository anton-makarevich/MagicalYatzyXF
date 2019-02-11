using System.Collections.Generic;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.Utils;

namespace Sanet.MagicalYatzy.Services.Game
{
    public class RulesService: IRulesService
    {
        public IEnumerable<Rules> GetAllRules()
        {
            return EnumUtils.GetValues<Rules>();
        }
    }
}