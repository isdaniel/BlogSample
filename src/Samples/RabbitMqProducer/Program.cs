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
            
            string exchangeName = "exchangeDirect";
            string routeKey = "Direct.Key1";
            string queueName = "DirectQueue";

            using (var connection = factory.CreateConnection())//创建通道
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false, null);

                channel.QueueBind(queueName,exchangeName,routeKey);
       

                Console.WriteLine("\nRabbitMQ連接成功,如需離開請按下Escape鍵");

                string input = string.Empty;
                do
                {
                    input = Console.ReadLine();

                    var sendBytes = Encoding.UTF8.GetBytes(input);
                    //發布訊息到RabbitMQ Server
                    channel.BasicPublish(exchangeName, routeKey, null, sendBytes);

                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }
        }
    }
}
