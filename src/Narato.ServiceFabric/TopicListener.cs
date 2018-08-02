﻿using Microsoft.ApplicationInsights;
using Microsoft.Azure.ServiceBus;
using Narato.ServiceFabric.ServiceBus.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Narato.ServiceFabric.ServiceBus
{
    public class TopicListener : ITopicListener
    {
        private Func<Message, CancellationToken, Task> _messageHandler;
        protected ISubscriptionClient _subscriptionClient;
        protected TelemetryClient _telemetryClient;

        public TopicListener(ISubscriptionClient subscriptionClient)
        {
            _telemetryClient = new TelemetryClient();
            _subscriptionClient = subscriptionClient;
        }

        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> value)
        {
            _messageHandler = value;
            RegisterOnMessageHandlerAndReceiveMessages(ProcessMessagesAsync);
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return _subscriptionClient.ClientId;
        }

        public void Abort()
        {
            Stop().Wait();
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await Stop();
        }

        private async Task Stop()
        {
            if (_subscriptionClient != null && !_subscriptionClient.IsClosedOrClosing)
            {
                await _subscriptionClient.CloseAsync();
            }
        }

        private void RegisterOnMessageHandlerAndReceiveMessages(Func<Message, CancellationToken, Task> handler)
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            _subscriptionClient.RegisterMessageHandler(handler, messageHandlerOptions);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            _telemetryClient.TrackException(exceptionReceivedEventArgs.Exception);
            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken cancellationToken)
        {
            // Process the message.
            await _messageHandler(message, cancellationToken);


             // The cancellationToken is used to determine if the subscriptionClient has already been closed.
            if (cancellationToken.IsCancellationRequested)
            {
                // If subscriptionClient has already been closed, call AbandonAsync() to avoid unnecessary exceptions.
                await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
            }
            else
            {
                // Complete the message so that it is not received again.
                // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }
    }
}
