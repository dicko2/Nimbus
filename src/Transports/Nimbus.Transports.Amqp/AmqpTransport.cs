using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.Amqp.Messages;
using Nimbus.Transports.Amqp.SendersAndRecievers;

namespace Nimbus.Transports.Amqp
{
    internal class AmqpTransport : INimbusTransport
    {
        private readonly ILogger _logger;
        private readonly Session _session;

        public AmqpTransport(ILogger logger)
        {
            _logger = logger;
            var address = new Address("amqp://admin:admin@macbook:5672");
            var connection = new Connection(address);
            _session = new Session(connection);

        }

        public Task TestConnection()
        {
            var task = new Task(() =>
                                {
                                    var isOpen = !_session.Connection.IsClosed;
                                    _logger.Debug("Connection is open - {0}", isOpen);
                                });
            
            return task;

        }

        public INimbusMessageSender GetQueueSender(string queuePath)
        {
            var factory = new MessageFactory();
            return new AmqpMessageSender(_session, queuePath, factory);
        }

        public INimbusMessageReceiver GetQueueReceiver(string queuePath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageSender GetTopicSender(string topicPath)
        {
            throw new NotImplementedException();
        }

        public INimbusMessageReceiver GetTopicReceiver(string topicPath, string subscriptionName, IFilterCondition filter)
        {
            throw new NotImplementedException();
        }
    }
}
