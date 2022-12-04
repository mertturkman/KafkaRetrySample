using System;
using System.Threading.Tasks;
using KafkaRetrySample.EventBus.Configs;
using KafkaRetrySample.EventBus.Consumer;
using KafkaRetrySample.EventBus.Producer;
using KafkaRetrySample.EventBus.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace KafkaRetrySample.Shovel
{
    class Program
    {
        static async Task Main(string[] args)
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
                    services.Configure<KafkaConsumerConfig>(hostContext.Configuration.GetSection(nameof(KafkaConsumerConfig)));
                    services.Configure<KafkaProducerConfig>(hostContext.Configuration.GetSection(nameof(KafkaProducerConfig)));

                    services.AddSingleton<ITopicRepository, TopicRepository>();
                    services.AddSingleton<IKafkaConsumer<string, string>, KafkaConsumer<string, string>>();
                    services.AddSingleton<IKafkaProducer<string, string>, KafkaProducer<string, string>>();

                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                        
                        q.ScheduleJob<Worker>(trigger => trigger
                            .StartNow()
                            .WithSimpleSchedule(x => x
                                .WithIntervalInMinutes(hostContext.Configuration.GetValue<int>("KafkaRetryIntervalMinute"))
                                .RepeatForever())
                        );
                    });

                    services.AddQuartzHostedService(options =>
                    {
                        options.WaitForJobsToComplete = true;
                    });
                })
                .Build()
                .Run();
        }
    }
}