using EasyNetQ;
using EasyNetQ.Topology;
using System;

namespace EzMQPublisher
{
    public class People {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string exchangeName = "my.Exchange";
            string routeKey = "my.routing";
            string queueName = "my.queue";


            using (var bus = RabbitHutch.CreateBus("host=127.0.0.1;port=5672;username=guest;password=guest").Advanced)
            {
                var exchange = bus.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                var queue = bus.QueueDeclare(queueName);
                bus.Bind(exchange, queue, routeKey);

                Console.WriteLine("請輸入訊息!");

                do
                {
                    string input = Console.ReadLine();

                    bus.Publish(exchange, "my.routing", false, new Message<string>(input));

                } while (Console.ReadKey().Key != ConsoleKey.Escape);
            }

            Console.ReadKey();
        }
    }
}
