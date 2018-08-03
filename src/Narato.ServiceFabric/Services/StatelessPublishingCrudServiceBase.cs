using Narato.ServiceFabric.Models;
using Narato.ServiceFabric.Persistence;
using Narato.ServiceFabric.ServiceBus.Interfaces;
using Narato.ServiceFabric.ServiceBus.Models;
using Narato.ServiceFabric.Services;
using System.Fabric;
using System.Threading.Tasks;

namespace Narato.ServiceFabric.ServiceBus.Services
{
    public class StatelessPublishingCrudServiceBase<TModel> : StatelessCrudServiceBase<TModel>, ICrudService<TModel>
        where TModel : ModelBase, new()
    {
        protected readonly ITopicSender _topicSender;

        public StatelessPublishingCrudServiceBase(StatelessServiceContext context, IPersistenceProvider<TModel> provider, bool softDeleteEnabled, ITopicSender topicSender)
            : base(context, provider, softDeleteEnabled)
        {
            _topicSender = topicSender;
        }

        public override async Task<TModel> CreateAsync(TModel modelToCreate)
        {
            var createdModel = await base.CreateAsync(modelToCreate);
            await SendTopicMessageAsync(TopicActions.Created, GetTopicMessagePayload(modelToCreate));
            return createdModel;
        }

        public override async Task<TModel> UpdateAsync(TModel modelToUpdate)
        {
            var updatedModel = await base.UpdateAsync(modelToUpdate);
            await SendTopicMessageAsync(TopicActions.Updated, GetTopicMessagePayload(modelToUpdate));
            return updatedModel;
        }

        public override async Task DeleteAsync(string key)
        {
            await base.DeleteAsync(key);
            await SendTopicMessageAsync(TopicActions.Deleted, GetTopicMessagePayload(new TModel { Key = key }));
        }

        protected virtual object GetTopicMessagePayload(TModel model)
        {
            return new
            {
                model.Key // inferred property name, cool huh? :)
            };
        }

        protected virtual async Task SendTopicMessageAsync(string action, object model)
        {
            var message = new TopicMessage() // why does TopicMessage exist in 2 seperate namespaces of the same library?
            {
                Action = action,
                Payload = model
            };
            await _topicSender.SendMessageAsync(message);
        }
    }
}
