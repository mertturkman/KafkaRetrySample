using System;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Configs;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.EventBus.Consumer
{
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>, IDisposable
    {
        private readonly KafkaConsumerConfig _kafkaConsumerConfig;
        private readonly KafkaConfig _kafkaConfig;
        private IConsumer<TKey, TValue> _consumer;

        public KafkaConsumer(IOptions<KafkaConfig> kafkaConfig, IOptions<KafkaConsumerConfig> kafkaConsumerConfig)
        {
            _kafkaConfig = kafkaConfig.Value;
            _kafkaConsumerConfig = kafkaConsumerConfig.Value;

            Initialize();
        }

        private void Initialize()
        {
            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = String.Join(",", _kafkaConfig.Brokers),
                GroupId = _kafkaConsumerConfig.GroupId,
                ClientId = _kafkaConsumerConfig.ClientId,
                SessionTimeoutMs = _kafkaConsumerConfig.SessionTimeoutMs,
                MessageMaxBytes =  _kafkaConsumerConfig.MessageMaxBytes,
                EnableAutoCommit = _kafkaConsumerConfig.EnableAutoCommit,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            
            ConsumerBuilder<TKey, TValue> consumerBuilder = new ConsumerBuilder<TKey, TValue>(consumerConfig);

            if (typeof(TKey) != typeof(string))
                consumerBuilder.SetKeyDeserializer(new JsonSerializer<TKey>());
            
            if (typeof(TValue) != typeof(string))
                consumerBuilder.SetValueDeserializer(new JsonSerializer<TValue>());

            _consumer = consumerBuilder.Build();
        }
        
        public void Subscribe(params string[] topics)
        {
            _consumer.Subscribe(topics[0]);
        }
        
        public void Unsubscribe()
        {
            _consumer.Unsubscribe();
        }
        
        public ConsumeResult<TKey, TValue> Consume()
        {
            return _consumer.Consume(_kafkaConsumerConfig.ConsumeTimeout);
        }

        public void Commit(ConsumeResult<TKey, TValue> message)
        {
            _consumer.Commit(message);
        }
        
        public void Commit()
        {
            _consumer.Commit();
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }
    }
}