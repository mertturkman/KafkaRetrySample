using Confluent.Kafka;

namespace KafkaRetrySample.EventBus.Consumer
{
    public interface IKafkaConsumer<TKey, TValue>
    {
        void Subscribe(params string[] topics);
        void Unsubscribe();
        ConsumeResult<TKey, TValue> Consume();
        void Commit(ConsumeResult<TKey, TValue> message);
        void Commit();
        void Dispose();
    }
}