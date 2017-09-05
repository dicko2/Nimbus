using Amqp;

namespace Nimbus.Transports.Amqp.Messages
{
    public class MessageFactory : IMessageFactory
    {
        public Message BuildAmqpMessage(NimbusMessage message)
        {

            var ampqMessage = new Message();


            return ampqMessage;
        }


        public NimbusMessage BuildNimbusMessage(Message message)
        {
            throw new System.NotImplementedException();
        }
    }


    public interface IMessageFactory
    {
        Message BuildAmqpMessage(NimbusMessage message);
        NimbusMessage BuildNimbusMessage(Message message);
    }
}