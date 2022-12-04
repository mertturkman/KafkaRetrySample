using System.Threading.Tasks;

namespace KafkaRetrySample.EventBus.Repositories
{
    public interface ITopicRepository
    {
        Task CreateTopic(string topicName);
        string[] GetTopics(string filter = null);
        string[] GetRetryTopics();
        string[] GetDeadTopics();
        Task CreateIfNotExistTopic(string topicName);
    }
}