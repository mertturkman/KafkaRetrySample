using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using KafkaRetrySample.EventBus.Configs;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.EventBus.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly IAdminClient _adminClient;
        private readonly TopicConfig _topicConfig;
        
        public TopicRepository(IOptions<KafkaConfig> kafkaConfig, IOptions<TopicConfig> topicConfig)
        {
            _adminClient = new AdminClientBuilder(
                    new AdminClientConfig
                    {
                        BootstrapServers = String.Join(",", kafkaConfig.Value.Brokers)
                    })
                .Build();
            
            _topicConfig = topicConfig.Value;
        }
        
        public async Task CreateIfNotExistTopic(string topicName)
        {
            if (!IsExist(topicName))
            {
                await CreateTopic(topicName);
            }
        }
        
        public async Task CreateTopic(string topicName)
        {
            await _adminClient.CreateTopicsAsync(new [] {
                new TopicSpecification
                {
                    Name = topicName,
                    ReplicationFactor = _topicConfig.DefaultReplicationFactor,
                    NumPartitions = _topicConfig.DefaultNumPartitions
                }
            });
        }

        public bool IsExist(string topicName)
        {
            return GetTopics().Contains(topicName);
        }
        
        public string[] GetTopics(string filter = null)
        {
            Metadata metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(_topicConfig.MetadataTimeout));

            return string.IsNullOrEmpty(filter)
                ? metadata.Topics.Select(t => t.Topic).ToArray()
                : metadata.Topics.Where(t => t.Topic.EndsWith(filter)).Select(t => t.Topic).ToArray();
        }
        
        public string[] GetRetryTopics()
        {
            return GetTopics(_topicConfig.RetrySuffix);
        }
        
        public string[] GetDeadTopics()
        {
            return GetTopics(_topicConfig.DeadSuffix);
        }

    }
}