using System;
using System.Linq.Expressions;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Configs;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaRetrySample.EventBus
{
    public static class KafkaBuilderExtensions
    {
        public static ProducerConfigurationBuilderExtensions producerConfig = new ProducerConfigurationBuilderExtensions();
        public static ConsumerConfigurationBuilderExtensions consumerConfig = new ConsumerConfigurationBuilderExtensions();
        
        public static IServiceCollection AddKafka(this IServiceCollection services, 
            Func<ProducerBuilderExtensions, ProducerBuilderExtensions> producerBuilder,
            Func<ConsumerBuilderExtensions, ConsumerBuilderExtensions> consumerBuilder)
        {
            return services;
        }

    }
    
    public class ProducerBuilderExtensions
    {
        public ProducerBuilderExtensions AddProducer(Func<ProducerConfigurationBuilderExtensions, ProducerConfigurationBuilderExtensions> producerConfigurationBuilder)
        {
            return this;
        }
    }
    
    public class ConsumerBuilderExtensions
    {
        public ConsumerBuilderExtensions AddConsumer(Func<ConsumerConfigurationBuilderExtensions, ConsumerConfigurationBuilderExtensions> consumerConfigurationBuilder)
        {
            return this;
        }
    }
    
    public class ProducerConfigurationBuilderExtensions : KafkaProducerConfig
    {
        public ProducerConfigurationBuilderExtensions Create => new ProducerConfigurationBuilderExtensions();

        public ProducerConfigurationBuilderExtensions WithMessageMaxBytes(int messageMaxBytes)
        {
            MessageMaxBytes = messageMaxBytes;
            return this;
        }
        
        public ProducerConfigurationBuilderExtensions WithMessageTimeoutMs(int messageTimeoutMs)
        {
            MessageTimeoutMs = messageTimeoutMs;
            return this;
        }
        
        public ProducerConfigurationBuilderExtensions WithAcks(string acks)
        {
            Acks = acks;
            return this;
        }
    }
    
    public class ConsumerConfigurationBuilderExtensions : KafkaConsumerConfig
    {
        public ConsumerConfigurationBuilderExtensions Create => new ConsumerConfigurationBuilderExtensions();

        public ConsumerConfigurationBuilderExtensions WithClientId(string clientId)
        {
            ClientId = clientId;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithGroupId(string groupId)
        {
            GroupId = groupId;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithConsumeTimeout(int consumeTimeout)
        {
            ConsumeTimeout = consumeTimeout;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithSessionTimeoutMs(int sessionTimeoutMs)
        {
            SessionTimeoutMs = sessionTimeoutMs;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithMessageMaxBytes(int messageMaxBytes)
        {
            MessageMaxBytes = messageMaxBytes;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithEnableAutoCommit(bool enableAutoCommit)
        {
            EnableAutoCommit = enableAutoCommit;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithConcurrencyLimit(int concurrencyLimit)
        {
            ConcurrencyLimit = concurrencyLimit;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }
        
        public ConsumerConfigurationBuilderExtensions WithConcurrencyRetryLimit(int concurrencyRetryLimit)
        {
            ConcurrencyRetryLimit = concurrencyRetryLimit;
            KafkaBuilderExtensions.consumerConfig = this;
            
            return this;
        }

    }
}