using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = "127.0.0.1",
                Port = 5672
            };

            string queueName = "my.queue";

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //channel.QueueBind
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                channel.BasicQos(0, 1, false);
                //接收到消息事件 consumer.IsRunning
                consumer.Received += (ch, e) =>
                {
                    var message = Encoding.UTF8.GetString(e.Body);

                    Console.WriteLine($"Queue:{queueName}收到資料： {message}");
                    channel.BasicAck(e.DeliveryTag, false);

                    if (e.BasicProperties.IsReplyToPresent())
                    {
                        var replayQueueName = e.BasicProperties.ReplyTo;
                        channel.BasicPublish("",
                                         replayQueueName,
                                         body: Encoding.UTF8.GetBytes("I got message, send to reply queue."));
                    }
                };

                channel.BasicConsume(queueName, false, consumer); 
                Console.WriteLine("接收訊息");
                Console.ReadKey();
            }

        }
    }
}
