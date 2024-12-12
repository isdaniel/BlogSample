using System;

namespace QueueWorkerEngine.RabbitMq
{
    public class RabbitMqSetting
    {
        public Uri GetUri()
        {
            return new Uri($"amqp://{UserName}:{Password}@{HostName}:{Port}");
        }

        public string GetUriWithoutPassword()
        {
            return $"amqp://{UserName}:*******@{HostName}:{Port}";
        }

        public ushort Port { get; set; }
        public string QueueName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public ushort PrefetchTaskCount { get; set; }

        public PoolType PoolType { get; set; }

        public PoolSetting[] PoolSettings { get; set; }
    }
}
