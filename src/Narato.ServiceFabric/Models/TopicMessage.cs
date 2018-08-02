namespace Narato.ServiceFabric.ServiceBus.Models
{
    public class TopicMessage
    {
        public string Action { get; set; }
        public dynamic Payload { get; set; }
    }
}