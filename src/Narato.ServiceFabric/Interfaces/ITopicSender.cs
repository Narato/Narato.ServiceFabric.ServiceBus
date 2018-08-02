using Narato.ServiceFabric.ServiceBus.Models;
using System.Threading.Tasks;

namespace Narato.ServiceFabric.ServiceBus.Interfaces
{
    public interface ITopicSender
    {
        Task SendMessageAsync(TopicMessage message);
    }
}
