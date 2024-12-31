using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitMqProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            //建立連接工廠
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost"
            };

            string exchangeName = "my.Exchange";
            string routeKey = "my.routing";
            string queueName = "my.queue";

            using (var connection = factory.CreateConnection())//创建通道
            using (var channel = connection.CreateModel())
            {
                #region 如果在RabbitMq手動建立可以忽略這段程式
                //建立一個Queue
                channel.QueueDeclare(queueName, true, false, false, null);
                //建立一個Exchange
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                //把Queue跟Exchange
                channel.QueueBind(queueName, exchangeName, routeKey);
                #endregion
                string replayQueueName  = "reply-queue";
                channel.QueueDeclare(replayQueueName, true, false, false, null);
                Console.WriteLine("\nRabbitMQ連接成功,如需離開請按下Escape鍵");

                string input = string.Empty;
                do
                {
                    input = Console.ReadLine();

                    var messageBytes = Encoding.UTF8.GetBytes(input);
                    var prop = channel.CreateBasicProperties();
                    prop.ReplyTo = replayQueueName;
                    channel.BasicPublish(exchange: exchangeName,
                                         routingKey: routeKey,
                                         prop,
                                         messageBytes);

                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
        }

    
    }
}
