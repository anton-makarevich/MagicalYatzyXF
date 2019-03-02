using Sanet.MagicalYatzy.Models.Chat;
using Xunit;

namespace MagicalYatzyTests.ModelTests.Chat
{
    public class ChatMessageTests
    {
        private readonly ChatMessage _sut = new ChatMessage();
        
        [Fact]
        public void HasSenderName()
        {
            const string senderName = "Me";
            _sut.SenderName = senderName;
            
            Assert.Equal(senderName,_sut.SenderName);
        }

        [Fact]
        public void HasReceiverName()
        {
            const string receiverName = "Him";
            _sut.ReceiverName = receiverName;
            
            Assert.Equal(receiverName,_sut.ReceiverName);
        }

        [Fact]
        public void HasMessage()
        {
            const string message = "Hi";
            _sut.Message = message;
            
            Assert.Equal(message, _sut.Message);
        }

        [Fact]
        public void CanBePrivate()
        {
            _sut.IsPrivate = true;
            
            Assert.True(_sut.IsPrivate);
        }
    }
}