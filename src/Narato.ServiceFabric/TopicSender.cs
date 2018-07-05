using Microsoft.ApplicationInsights;
using Microsoft.Azure.ServiceBus;
using Narato.ResponseMiddleware.Models.Exceptions;
using Narato.ServiceFabric.ServiceBus.Interfaces;
using Narato.ServiceFabric.ServiceBus.Models;
using System;
using System.Threading.Tasks;

namespace Narato.ServiceFabric.ServiceBus
{
    public class TopicSender : ITopicSender
    {
        private readonly TopicClient _topicClient;
        protected TelemetryClient _telemetryClient;

        public TopicSender(string connectionString, string topicName)
        {
            _topicClient = new TopicClient(connectionString, topicName);
            _telemetryClient = new TelemetryClient();
        }

        public async Task SendMessageAsync(TopicMessage message)
        {
            try
            {
                await _topicClient.SendAsync(new Message(ObjectSerializer.Serialize(message)));
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                throw new ExceptionWithFeedback("EWF", "Something went wrong while posting to the servicebus: " + ex.Message);
            }
        }

    }
}
