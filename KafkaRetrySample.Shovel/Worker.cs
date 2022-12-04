using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Consumer;
using KafkaRetrySample.EventBus.Producer;
using KafkaRetrySample.EventBus.Repositories;
using Quartz;

namespace KafkaRetrySample.Shovel
{
    public class Worker : IJob
    {
        private readonly IKafkaConsumer<string, string> _consumer;
        private readonly IKafkaProducer<string, string> _producer;
        private readonly ITopicRepository _topicRepository;
        
        public Worker(IKafkaConsumer<string, string> consumer, IKafkaProducer<string, string> producer, ITopicRepository topicRepository)
        {
            _consumer = consumer;
            _producer = producer;
            _topicRepository = topicRepository;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                string[] retryTopics = _topicRepository.GetDeadTopics();
                _consumer.Subscribe(retryTopics);

                while (true)
                {
                    var consumeResult = _consumer.Consume();

                    if (consumeResult == null)
                    {
                        Console.WriteLine("Reached end of dead topics.");
                        break;
                    }

                    _producer.ProduceToRetry(consumeResult.Topic, consumeResult.Message.Key,
                        consumeResult.Message.Value, consumeResult.Message.Headers);
                    _consumer.Commit(consumeResult);
                }
                
                _consumer.Unsubscribe();
            }
            catch (KafkaException e)
            {
                Console.WriteLine($"Kafka failed: {e.Error.Reason}");
            }

            return Task.CompletedTask;
        }
    }
}