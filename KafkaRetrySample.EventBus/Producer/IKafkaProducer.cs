using Confluent.Kafka;

namespace KafkaRetrySample.EventBus.Producer
{
    public interface IKafkaProducer<TKey, TValue>
    {
        void ProduceToDead(string topicName, TKey key, TValue value, Headers headers);
        void ProduceToRetry(string topicName, TKey key, TValue value, Headers headers);
        void Produce(TKey key, TValue value, string topic, Headers headers = default);
    }
}