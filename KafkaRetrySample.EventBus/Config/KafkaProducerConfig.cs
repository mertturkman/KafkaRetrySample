namespace KafkaRetrySample.EventBus.Configs
{
    public class KafkaProducerConfig
    {
        public int MessageMaxBytes { get; set; }
        public int MessageTimeoutMs { get; set; }
        public string Acks { get; set; }
    }
}