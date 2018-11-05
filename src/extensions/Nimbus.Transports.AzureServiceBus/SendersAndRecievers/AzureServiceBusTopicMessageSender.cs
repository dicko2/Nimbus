using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Infrastructure.Retries;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.QueueManagement;
using Nimbus.Extensions;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusTopicMessageSender : INimbusMessageSender, IDisposable
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly ILogger _logger;

        private TopicClient _topicClient;
        private readonly IRetry _retry;

        public AzureServiceBusTopicMessageSender(IBrokeredMessageFactory brokeredMessageFactory, ILogger logger, IQueueManager queueManager, IRetry retry, string topicPath)
        {
            _queueManager = queueManager;
            _retry = retry;
            _topicPath = topicPath;
            _logger = logger;
            _brokeredMessageFactory = brokeredMessageFactory;
        }

        public async Task Send(NimbusMessage message)
        {
            await _retry.DoAsync(async () =>
                                       {
                                           var brokeredMessage = await _brokeredMessageFactory.BuildBrokeredMessage(message);

                                           var topicClient = GetTopicClient();
                                           try
                                           {
                                               await topicClient.SendAsync(brokeredMessage);
                                           }
                                           catch (MessagingEntityNotFoundException exc)
                                           {
                                               _logger.Error(exc, "The referenced topic path {TopicPath} no longer exists", _topicPath);
                                               await _queueManager.MarkTopicAsNonExistent(_topicPath);
                                               DiscardTopicClient();
                                               throw;
                                           }
                                           catch (Exception)
                                           {
                                               DiscardTopicClient();
                                               throw;
                                           }
                                       },
                                 "Sending message to topic").ConfigureAwaitFalse();
        }

        private TopicClient GetTopicClient()
        {
            if (_topicClient != null) return _topicClient;

            _topicClient = _queueManager.CreateTopicSender(_topicPath).Result;
            return _topicClient;
        }

        private void DiscardTopicClient()
        {
            var topicClient = _topicClient;
            _topicClient = null;

            if (topicClient == null) return;

            try
            {
                _logger.Debug("Discarding message sender for {TopicPath}", _topicPath);
                topicClient.CloseAsync().RunSynchronously();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Failed to close TopicClient instance before discarding it.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            DiscardTopicClient();
        }
    }
}