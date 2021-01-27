using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Nimbus.Transports.AzureServiceBus.BrokeredMessages
{
    internal interface IBrokeredMessageFactory
    {
        Task<Message> BuildBrokeredMessage(NimbusMessage message);
        Task<NimbusMessage> BuildNimbusMessage(Message message);
    }
}