using System;
using Confluent.Kafka;

namespace KafkaRetrySample.EventBus
{
    public class Utility
    {
        public static void SetRetryCount(Headers headers, int value)
        {
            headers.Remove("RetryCount");
            headers.Add("RetryCount", BitConverter.GetBytes(value));
        }
    }
}