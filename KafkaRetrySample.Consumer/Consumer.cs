using System;
using System.Threading.Tasks;
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Consumer;
using KafkaRetrySample.EventBus.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.Consumer
{
    public class Consumer : BaseConsumer<string>
    {
        public Consumer(IKafkaConsumer<string, string> consumer, IKafkaConsumer<string, string> retryConsumer,
            IKafkaProducer<string, string> customProducer,
            IOptions<TopicConfig> topicConfig, IOptions<KafkaConsumerConfig> consumerConfig, ILogger<Consumer> logger) : base(
            consumer, retryConsumer, customProducer, topicConfig, consumerConfig, logger)
        {
        }

        protected override async Task Consume(string message)
        {
            if (message.Contains("error"))
            {
                throw new Exception($"Retry queue message: {message}");
            }

            Console.Out.WriteLine(message);
        }
    }
}