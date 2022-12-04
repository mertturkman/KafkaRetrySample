using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.Producer
{
    public class Producer : BackgroundService
    {
        private readonly IKafkaProducer<string, string> _producer;
        private readonly TopicConfig _topicConfig;
        
        public Producer(IKafkaProducer<string, string> producer, IOptions<TopicConfig> topicConfig)
        {
            _producer = producer;
            _topicConfig = topicConfig.Value;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string value = null;
            while (value != "exit")
            {
                try
                {
                    Console.Write("Enter Key: ");
                    string key = Console.ReadLine();

                    Console.Write("Enter Message: ");
                    value = Console.ReadLine();
                        
                    _producer.Produce(key, value, _topicConfig.Topics[0]);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
            return Task.CompletedTask;
        }
    }
}