using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Producer;
using KafkaRetrySample.EventBus.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaRetrySample.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    IHostEnvironment env = hostContext.HostingEnvironment;
                    config.AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json")
                        .AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<KafkaConfig>(hostContext.Configuration.GetSection(nameof(KafkaConfig)));
                    services.Configure<TopicConfig>(hostContext.Configuration.GetSection(nameof(TopicConfig)));
                    services.Configure<KafkaProducerConfig>(hostContext.Configuration.GetSection(nameof(KafkaProducerConfig)));

                    services.AddSingleton<ITopicRepository, TopicRepository>();
                    services.AddSingleton<IKafkaProducer<string, string>, KafkaProducer<string, string>>();

                    services.AddHostedService<Producer>();
                })
                .Build()
                .Run();
        }
    }
}