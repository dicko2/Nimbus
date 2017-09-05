using System;
using System.Threading;
using System.Threading.Tasks;
using Amqp;
using Nimbus.Configuration.Settings;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;

namespace Nimbus.Transports.Amqp.SendersAndRecievers
{
    internal class AmqpMessageReciever : ThrottlingMessageReceiver
    {
        public AmqpMessageReciever(ConcurrentHandlerLimitSetting concurrentHandlerLimit, IGlobalHandlerThrottle globalHandlerThrottle, ILogger logger) : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
        }

        protected override Task WarmUp()
        {
            throw new NotImplementedException();
        }

        protected override Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}