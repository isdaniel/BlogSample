using Confluent.Kafka;

var bootstrapServers = "localhost:9092,localhost:9093,localhost:9094";
var topic = "example-topic";
Console.WriteLine("group name:");
var groupId = Console.ReadLine();
var config = new ConsumerConfig
{
    BootstrapServers = bootstrapServers,
    GroupId = groupId,
    AutoOffsetReset = AutoOffsetReset.Earliest, // Read messages from the beginning if no offset exists
    EnableAutoCommit = false,  // Automatically commit offsets
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

Console.WriteLine("Connecting to Kafka...");

try
{
    // Subscribe to the topic
    consumer.Subscribe(topic);

    Console.WriteLine($"Subscribed to topic: {topic}");
    Console.WriteLine("Press Ctrl+C to exit.");

    while (true)
    {
        try
        {
            // Consume messages
            var result = consumer.Consume(CancellationToken.None);
            Console.WriteLine($"Message received: {result.Message.Value} | Partition: {result.Partition} | Offset: {result.Offset}");

            // Commit the offset manually
            consumer.Commit(result);
            Console.WriteLine($"Offset committed: {result.Offset}");
        }
        catch (ConsumeException ex)
        {
            Console.WriteLine($"Error consuming message: {ex.Error.Reason}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    consumer.Close();
}
