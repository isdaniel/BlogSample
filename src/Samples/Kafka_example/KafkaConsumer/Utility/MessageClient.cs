// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

public class MessageClient<TKey> : IDisposable
{
    protected MessageClientOptions<TKey> _options { get; set; }
    private IProducer<TKey, string> _producer;
    private IConsumer<TKey, string> _consumer;
    private IAdminClient _adminClient;
    public MessageClient(
        MessageClientOptions<TKey> options)
    {
        this._options = options;

        _producer = _options.GetProducer();
        _consumer = _options.GetConsumer();
        _adminClient = new AdminClientBuilder(config: new AdminClientConfig { BootstrapServers = EnvironmentVAR.HOSTNAME, Acks = Acks.All }).Build();

        _consumer.Subscribe(_options.Topic);
    }

    public virtual async ValueTask<string> PublishMessageAsync(
        string messageBody,
        string? correlationId = null,
        Dictionary<string, string>? messageHeaders = null)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        var replyHeaders = default(Headers);
        if (messageHeaders != null)
        {
            replyHeaders = new Headers();
            foreach (var item in messageHeaders)
            {
                replyHeaders.Add(item.Key, Encoding.UTF8.GetBytes(item.Value));

                if (item.Key == "ReplyTo" && !string.IsNullOrWhiteSpace(item.Value))
                {
                    await CreateTopic(item.Value);
                }
            }
        }

        await _producer.ProduceAsync(_options.Topic, new Message<TKey, string>()
        {
            Value = messageBody,
            Headers = replyHeaders
        }).ConfigureAwait(false);

        return correlationId;
    }

    public string ConsumeMessage()
    {
        var corrId = Guid.NewGuid().ToString("N");
        Console.WriteLine($"[{corrId}]Start ConsumeMessage: {DateTime.Now:yyyy:MM:dd hh:mm:ss}");
        var responseMsg = _consumer.Consume(3000);
        while (responseMsg == null)
        {
            responseMsg = _consumer.Consume(3000);
        }
        var res = responseMsg.Message.Value;
        Console.WriteLine($"[{corrId}]End ConsumeMessage: {DateTime.Now:yyyy:MM:dd hh:mm:ss}");
        _consumer.Commit(responseMsg);
        return res;
    }

    public async Task CreateTopic(string topic)
    {
        try
        {
            await _adminClient.CreateTopicsAsync(new TopicSpecification[]
        {
            new TopicSpecification
            {
                Name = topic,
                NumPartitions = 4
            }
        }).ConfigureAwait(false);
        }
        catch (CreateTopicsException ex) when (
           ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {

            //ignore
        }
    }

    public async Task DeleteTopics(string[] topics)
    {
        await _adminClient.DeleteTopicsAsync(topics).ConfigureAwait(false);
    }

    public virtual void Dispose()
    {
        _producer.Dispose();
        _consumer.Dispose();
        _adminClient.Dispose();
    }
}
