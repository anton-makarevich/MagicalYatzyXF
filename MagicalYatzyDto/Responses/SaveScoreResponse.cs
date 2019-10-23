using Sanet.MagicalYatzy.Dto.Models;
using Sanet.MagicalYatzy.Dto.Responses.Base;

namespace Sanet.MagicalYatzy.Dto.Responses
{
    public class SaveScoreResponse:ResponseBase
    {
        public PlayerScore? Score { get; set; }
    }
}