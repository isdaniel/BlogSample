using System;
using System.Threading.Tasks;
using Confluent.Kafka;

class KafkaPublisher
{
    static async Task Main(string[] args)
    {
        // Kafka broker address
        var bootstrapServers = "localhost:9092,localhost:9093,localhost:9094";
        var topic = "example-topic";

        // Producer configuration
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        // Create the producer
        using var producer = new ProducerBuilder<Null, string>(config).Build();

        Console.WriteLine("Enter messages to send to Kafka (type 'exit' to quit):");
        
        while (true)
        {
            // Read a message from the console
            var message = Console.ReadLine();

            if (message.Equals("exit",StringComparison.CurrentCultureIgnoreCase))
                break;

            try
            {
                // Produce the message
                var deliveryResult = await producer.ProduceAsync(topic, new Message<Null, string>
                {
                    Value = message
                });

                Console.WriteLine($"Message '{message}' delivered to: {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine($"Failed to deliver message: {ex.Error.Reason}");
            }
        }

        Console.WriteLine("Kafka publisher stopped.");
    }
}
