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
            
            string exchangeName = "exchange2";
            string routeKey = "hello";
            string queueName = "q1";

            using (var connection = factory.CreateConnection())//创建通道
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, false, false, null);

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
