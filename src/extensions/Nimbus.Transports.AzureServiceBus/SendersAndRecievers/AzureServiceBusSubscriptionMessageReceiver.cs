using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Filtering.Attributes;
using Nimbus.Filtering.Conditions;
using Nimbus.Infrastructure;
using Nimbus.Infrastructure.MessageSendersAndReceivers;
using Nimbus.Transports.AzureServiceBus.BrokeredMessages;
using Nimbus.Transports.AzureServiceBus.Filtering;
using Nimbus.Transports.AzureServiceBus.QueueManagement;

namespace Nimbus.Transports.AzureServiceBus.SendersAndRecievers
{
    internal class AzureServiceBusSubscriptionMessageReceiver : ThrottlingMessageReceiver
    {
        private readonly IBrokeredMessageFactory _brokeredMessageFactory;
        private readonly ILogger _logger;
        private readonly IQueueManager _queueManager;
        private readonly string _topicPath;
        private readonly string _subscriptionName;
        private readonly IFilterCondition _filterCondition;
        private SubscriptionClient _subscriptionClient;
        private BlockingCollection<Message> _queue= new BlockingCollection<Message>();
        public AzureServiceBusSubscriptionMessageReceiver(IQueueManager queueManager,
                                                          string topicPath,
                                                          string subscriptionName,
                                                          IFilterCondition filterCondition,
                                                          ConcurrentHandlerLimitSetting concurrentHandlerLimit,
                                                          IBrokeredMessageFactory brokeredMessageFactory,
                                                          IGlobalHandlerThrottle globalHandlerThrottle,
                                                          ILogger logger)
            : base(concurrentHandlerLimit, globalHandlerThrottle, logger)
        {
            _queueManager = queueManager;
            _topicPath = topicPath;
            _subscriptionName = subscriptionName;
            _filterCondition = filterCondition;
            _brokeredMessageFactory = brokeredMessageFactory;
            _logger = logger;

            _subscriptionClient = GetSubscriptionClient().Result;
            _subscriptionClient.RegisterMessageHandler(OnMessage, OnMessageError);
        }

        public override string ToString()
        {
            return "{0}/{1}".FormatWith(_topicPath, _subscriptionName);
        }

        protected override async Task WarmUp()
        {
            await GetSubscriptionClient();
        }

        private async Task CancellationTask(SemaphoreSlim cancellationSemaphore, CancellationToken cancellationToken)
        {
            try
            {
                await cancellationSemaphore.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override async Task<NimbusMessage> Fetch(CancellationToken cancellationToken)
        {
            try
            {
                if(_queue.TryTake(out var brokeredMessage))
                { 
                    var nimbusMessage = await _brokeredMessageFactory.BuildNimbusMessage(brokeredMessage);
                    nimbusMessage.Properties[MessagePropertyKeys.RedeliveryToSubscriptionName] = _subscriptionName;
                    await _subscriptionClient.CompleteAsync(brokeredMessage.SystemProperties.LockToken);
                    return nimbusMessage;
                }
            }
            catch (MessagingEntityNotFoundException exc)
            {
                _logger.Error(exc, "The referenced topic subscription {TopicPath}/{SubscriptionName} no longer exists", _topicPath, _subscriptionName);
                await _queueManager.MarkSubscriptionAsNonExistent(_topicPath, _subscriptionName);
                DiscardSubscriptionClient();
                throw;
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Messaging operation failed. Discarding message receiver.");
                DiscardSubscriptionClient();
                throw;
            }
            return null;
        }

        private async Task OnMessageError(ExceptionReceivedEventArgs arg)
        {
            _logger.Error(arg.Exception, "OnMessageError");
        }

        private async Task OnMessage(Message arg1, CancellationToken arg2)
        {
            _queue.Add(arg1);
        }

        private async Task<SubscriptionClient> GetSubscriptionClient()
        {
            if (_subscriptionClient != null) return _subscriptionClient;

            _subscriptionClient = await _queueManager.CreateSubscriptionReceiver(_topicPath, _subscriptionName, _filterCondition);
            _subscriptionClient.PrefetchCount = ConcurrentHandlerLimit;
            return _subscriptionClient;
        }

        private void DiscardSubscriptionClient()
        {
            var subscriptionClient = _subscriptionClient;
            _subscriptionClient = null;

            if (subscriptionClient == null) return;

            try
            {
                subscriptionClient.CloseAsync().RunSynchronously();
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "An exception occurred while closing a SubscriptionClient.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing) return;
                if (_queue.Count != 0)
                {
                    // any messages that remain in the queue in memory should be unlock in azure
                    _queue.GetConsumingEnumerable()
                        .Select(async x => await _subscriptionClient
                            .AbandonAsync(x.SystemProperties.LockToken));
                }
                DiscardSubscriptionClient();
            }
            catch (MessagingEntityNotFoundException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}