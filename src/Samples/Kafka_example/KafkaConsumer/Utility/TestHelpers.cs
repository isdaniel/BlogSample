// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.RegularExpressions;
using Confluent.Kafka;

public static class TestHelpers
{
//    public static TResult WaitForMessageResult<TResult>(
//        string replyQueue, Func<string, TResult> action, string[] deleteQueue = null)
//    {
//        using (MessageClient<Null> messageClient = new MessageClient<Null>(new MessageClientOptions<Null>()
//        {
//            ProducerCfg = new ProducerConfig()
//            {
//                BootstrapServers = EnvironmentVAR.HOSTNAME,
//                Acks = Acks.All,
//                EnableIdempotence = true
//            },
//            ConsumerCfg = new ConsumerConfig()
//            {
//                BootstrapServers = EnvironmentVAR.HOSTNAME,
//                GroupId = EnvironmentVAR.GROUPID,
//                AutoOffsetReset = AutoOffsetReset.Earliest,
//                EnableAutoCommit = false
//            },
//            Topic = replyQueue,
//        }))
//        {

//            Console.WriteLine($"replyQueue {replyQueue}");
//            var res = action();
//            return messageClient.ConsumeMessage();
//        }
//    }

    public static async Task SendingMessageAsync(string bodyMessage, string correlationId, string queueName, Dictionary<string, string> header = null)
    {
        using (MessageClient<Null> messageClient = new MessageClient<Null>(new MessageClientOptions<Null>()
        {
            ProducerCfg = new ProducerConfig()
            {
                BootstrapServers = EnvironmentVAR.HOSTNAME,
                Acks = Acks.All,
                EnableIdempotence = true
            },
            ConsumerCfg = new ConsumerConfig()
            {
                BootstrapServers = EnvironmentVAR.HOSTNAME,
                GroupId = EnvironmentVAR.GROUPID,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            },
            Topic = queueName
        }))
        {
            await messageClient.PublishMessageAsync(bodyMessage, correlationId, header).ConfigureAwait(false);
        }
    }
}
