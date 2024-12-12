using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueWorkerEngine;
using QueueWorkerEngine.RabbitMq;
using Microsoft.Extensions.Logging;
using System;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<BackgroundWorker>();
                services.AddSingleton<RabbitMqWorkerBase, RabbitMqGroupWorker>();
                services.AddSingleton<IPoolFactory, WorkerPoolFactory>();
                services.AddSingleton(new RabbitMqSetting
                {
                    QueueName = Environment.GetEnvironmentVariable("QUEUENAME"),
                    UserName = Environment.GetEnvironmentVariable("USERNAME") ?? "guest",
                    Password = Environment.GetEnvironmentVariable("PASSWORD") ?? "guest",
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME"),
                    Port = ushort.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out ushort p) ? p : (ushort)5672,
                    PrefetchTaskCount = ushort.TryParse(Environment.GetEnvironmentVariable("PREFETCHTASKCOUNT"), out ushort result) ? result : (ushort)1,
                    PoolSettings = new PoolSetting[] //which can read from setting files.
                        {
                                new PoolSetting(){WorkUnitCount = 3,Group = "groupA" , FileName = "dotnet",Arguments = @"./Process/Group/Client.dll"},
                                new PoolSetting(){WorkUnitCount = 3,Group = "groupB" , FileName = "dotnet",Arguments = @"./Process/Group/Client.dll"}
                        },
                    //default threadPool
                    PoolType = Enum.TryParse(Environment.GetEnvironmentVariable("POOL_TYPE"), out PoolType poolType) ? poolType : PoolType.Thread
                });
            });
}