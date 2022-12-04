using System.Collections.Generic;

namespace KafkaRetrySample.EventBus.Configs
{
    public class TopicConfig
    {
        public string[] Topics { get; set; }
        public string RetrySuffix { get; set; }
        public string DeadSuffix { get; set; }
        public int MetadataTimeout { get; set; }
        public short DefaultReplicationFactor { get; set; }
        public int DefaultNumPartitions { get; set; }
        public int MaxRetryCount { get; set; }
        public int RetryDelaySecond { get; set; }
    }
}