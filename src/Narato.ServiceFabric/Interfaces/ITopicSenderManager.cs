using Narato.ServiceFabric.ServiceBus.Models;
using System.Threading.Tasks;

namespace Narato.ServiceFabric.ServiceBus.Interfaces
{
    public interface ITopicSenderManager
    {
        Task EnqueueMessageAsync(TopicMessage serviceBusMessage);
    }
}
