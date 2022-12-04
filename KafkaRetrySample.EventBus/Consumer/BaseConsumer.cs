using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaRetrySample.EventBus.Consumer
{
    public abstract class BaseConsumer<TMessage> : BackgroundService
    {
        private readonly IKafkaConsumer<string, TMessage> _consumer;
        private readonly IKafkaConsumer<string, TMessage> _retryConsumer;
        private readonly IKafkaProducer<string, TMessage> _producer;
        private readonly TopicConfig _topicConfig;
        private readonly KafkaConsumerConfig _consumerConfig;
        private readonly ILogger _logger;

        protected BaseConsumer(IKafkaConsumer<string, TMessage> consumer,
            IKafkaConsumer<string, TMessage> retryConsumer,
            IKafkaProducer<string, TMessage> producer,
            IOptions<TopicConfig> topicConfig,
            IOptions<KafkaConsumerConfig> consumerConfig,
            ILogger logger)
        {
            _consumer = consumer;
            _retryConsumer = retryConsumer;
            _producer = producer;
            _topicConfig = topicConfig.Value;
            _consumerConfig = consumerConfig.Value;
            _logger = logger;
        }

        protected abstract Task Consume(TMessage message);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task runConsumer = Task.Factory.StartNew(() =>
                PrepareConsumer(_consumer, _topicConfig.Topics, _consumerConfig.ConcurrencyLimit, false), TaskCreationOptions.AttachedToParent);
            
            Task runRetryConsumer = Task.Factory.StartNew(() => PrepareConsumer(_retryConsumer,
                _topicConfig.Topics.Select(topic => string.Concat(topic, "_", _topicConfig.RetrySuffix)).ToArray(),
                _consumerConfig.ConcurrencyRetryLimit, true), TaskCreationOptions.AttachedToParent);

            await Task.WhenAll(runConsumer, runRetryConsumer);
        }

        private async Task PrepareConsumer(IKafkaConsumer<string, TMessage> consumer, string[] topics,
            int concurrencyLimit, bool isRetryTopics)
        {
            consumer.Subscribe(topics);
            await ConcurrencyConsume(consumer, concurrencyLimit, isRetryTopics);
        }

        private async Task ConcurrencyConsume(IKafkaConsumer<string, TMessage> consumer, int concurrencyLimit,
            bool isRetryTopics)
        {
            ConsumeResult<string, TMessage> consumeResult = null;
            ThreadSafeList<Task> tasks = new ThreadSafeList<Task>();
            Dictionary<int, ConsumeResult<string, TMessage>> consumeResults =
                new Dictionary<int, ConsumeResult<string, TMessage>>();

            while (true)
            {
                try
                {
                    while ((tasks.Count < concurrencyLimit && consumeResult != null) || tasks.Count <= 0)
                    {
                        consumeResult = consumer.Consume();
                        if (consumeResult != null)
                        {
                            if (isRetryTopics)
                            {
                                long sleepMs = Math.Max(0,
                                    consumeResult.Message.Timestamp.UnixTimestampMs +
                                    _topicConfig.RetryDelaySecond * 1000 -
                                    Timestamp.DateTimeToUnixTimestampMs(DateTime.UtcNow));

                                await Task.Delay((int)sleepMs);
                            }

                            var task = Task.Factory.StartNew(() => Consume(consumeResult.Message.Value),
                                TaskCreationOptions.AttachedToParent);
                            tasks.Add(task);
                            consumeResults.Add(task.Id, consumeResult);
                        }
                    }

                    await Task.WhenAll(tasks.ToArray());

                    foreach (var task in tasks)
                    {
                        var taskResult = (task as Task<Task>).Result;
                        consumeResult = consumeResults[task.Id];
                        if (taskResult.IsFaulted)
                        {
                            _logger.LogError("Consume exception occured: {Message} {StackTrace}", 
                                taskResult.Exception.Message, taskResult.Exception.StackTrace);

                            _producer.ProduceToRetry(consumeResult.Topic, consumeResult.Message.Key,
                                consumeResult.Message.Value, consumeResult.Message.Headers);
                        }

                        _consumer.Commit(consumeResult);
                    }

                    tasks.Clear();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consume exception occured: {StackTrace}", ex.StackTrace);
                }
            }
        }
    }
}