
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Consumer;
using KafkaRetrySample.EventBus.Producer;
using KafkaRetrySample.EventBus.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaRetrySample.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<KafkaConfig>(hostContext.Configuration.GetSection(nameof(KafkaConfig)));
                    services.Configure<TopicConfig>(hostContext.Configuration.GetSection(nameof(TopicConfig)));
                    services.Configure<KafkaConsumerConfig>(hostContext.Configuration.GetSection(nameof(KafkaConsumerConfig)));
                    services.Configure<KafkaProducerConfig>(hostContext.Configuration.GetSection(nameof(KafkaProducerConfig)));
                    services.AddSingleton<ITopicRepository, TopicRepository>();
                    
                    services.AddTransient<IKafkaConsumer<string, string>, KafkaConsumer<string, string>>();
                    services.AddTransient<IKafkaConsumer<string, string>, KafkaConsumer<string, string>>();
                    services.AddTransient<IKafkaProducer<string, string>, KafkaProducer<string, string>>();
                    
                    services.AddHostedService<Consumer>();
                })
                .Build()
                .Run();
        }
    }
}