using System.Threading.Tasks;
using Amqp;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Amqp.Messages;

namespace Nimbus.Transports.Amqp.SendersAndRecievers
{
    public class AmqpMessageSender : INimbusMessageSender
    {
        private readonly IMessageFactory _messageFactory;
        private readonly SenderLink _senderLink;

        public AmqpMessageSender(Session session, string queuePath, IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
            var linkName = "link-" + queuePath;
            _senderLink = new SenderLink(session, linkName, queuePath);
        }

        public Task Send(NimbusMessage message)
        {
            var amqpMessage = _messageFactory.BuildAmqpMessage(message);
            return _senderLink.SendAsync(amqpMessage);
        }
    }
}