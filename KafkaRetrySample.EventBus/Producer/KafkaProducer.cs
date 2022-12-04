using System;
using System.Buffers.Binary;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Repositories;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.EventBus.Producer
{
    public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
    {
        private readonly TopicConfig _topicConfig;
        private readonly KafkaConfig _kafkaConfig;
        private readonly KafkaProducerConfig _producerConfig;
        private readonly ITopicRepository _topicRepository;
        private IProducer<TKey, TValue> _producer;

        public KafkaProducer(IOptions<KafkaProducerConfig> producerConfig, IOptions<KafkaConfig> kafkaConfig,
            IOptions<TopicConfig> topicConfig, ITopicRepository topicRepository)
        {
            _topicConfig = topicConfig.Value;
            _producerConfig = producerConfig.Value;
            _kafkaConfig = kafkaConfig.Value;
            _topicRepository = topicRepository;

            Initialize();
        }

        private void Initialize()
        {
            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = String.Join(",", _kafkaConfig.Brokers),
                MessageMaxBytes = _producerConfig.MessageMaxBytes,
                MessageTimeoutMs = _producerConfig.MessageTimeoutMs,
                Acks = (Acks)Enum.Parse(typeof(Acks), _producerConfig.Acks, true),
            };

            ProducerBuilder<TKey, TValue> producerBuilder = new ProducerBuilder<TKey, TValue>(producerConfig);

            if (typeof(TKey) != typeof(string))
                producerBuilder.SetKeySerializer(new JsonSerializer<TKey>());

            if (typeof(TValue) != typeof(string))
                producerBuilder.SetValueSerializer(new JsonSerializer<TValue>());

            foreach (var topicName in _topicConfig.Topics)
            {
                _topicRepository.CreateIfNotExistTopic(topicName);
            }

            _producer = producerBuilder.Build();
        }

        public void ProduceToDead(string topicName, TKey key, TValue value, Headers headers)
        {
            topicName = topicName.Replace(_topicConfig.DeadSuffix, string.Empty)
                .Replace(_topicConfig.RetrySuffix, string.Empty);
            Produce(key, value, $"{topicName}_{_topicConfig.DeadSuffix}", headers);
        }

        public void ProduceToRetry(string topicName, TKey key, TValue value, Headers headers)
        {
            byte[] headerValue;

            topicName = topicName.Replace($"_{_topicConfig.DeadSuffix}", string.Empty)
                .Replace($"_{_topicConfig.RetrySuffix}", string.Empty);
            
            if (headers.TryGetLastBytes("RetryCount", out headerValue))
            {
                int retryCount = BinaryPrimitives.ReadInt32LittleEndian(headerValue);
                if (retryCount >= _topicConfig.MaxRetryCount)
                {
                    Utility.SetRetryCount(headers, 1);
                    ProduceToDead(topicName, key, value, headers);
                }
                else
                {
                    Utility.SetRetryCount(headers, retryCount + 1);
                    Produce(key, value, $"{topicName}_{_topicConfig.RetrySuffix}", headers);
                }
            }
            else
            {
                Utility.SetRetryCount(headers, 1);
                Produce(key, value, $"{topicName}_{_topicConfig.RetrySuffix}", headers);
            }
        }

        public void Produce(TKey key, TValue value, string topic, Headers headers = default)
        {
            _producer.Produce(topic, new Message<TKey, TValue> { Key = key, Value = value, Headers = headers });
        }
    }
}