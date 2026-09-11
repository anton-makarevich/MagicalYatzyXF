namespace Sanet.MagicalYatzy.Models.Chat
{
    public class ChatMessage
    {
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public bool IsPrivate { get; set; }
        public string Message { get; set; }
    }
}
