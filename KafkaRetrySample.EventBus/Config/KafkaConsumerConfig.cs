namespace KafkaRetrySample.EventBus.Configs
{
    public class KafkaConsumerConfig
    {
        public string ClientId { get; set; }
        public string GroupId { get; set; }
        public int ConsumeTimeout { get; set; }
        public int SessionTimeoutMs { get; set; }
        public int MessageMaxBytes { get; set; }
        public bool EnableAutoCommit { get; set; }
        public int ConcurrencyLimit { get; set; }
        public int ConcurrencyRetryLimit { get; set; }
    }
}