namespace Sanet.MagicalYatzy.Dto.Responses.Base
{
    public abstract class ResponseBase
    {
        public string? Message { get; set; }
        public int? ErrorCode { get; set; }
    }
}