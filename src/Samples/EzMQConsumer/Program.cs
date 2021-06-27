using EasyNetQ;
using EasyNetQ.Topology;
using System;
using System.Threading.Tasks;

namespace EzMQConsumer
{
    public class People
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            string queueName = "my.queue";

            using (var bus = RabbitHutch.CreateBus("host=127.0.0.1;port=5672;username=guest;password=guest"))
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        bus.SendReceive.Receive<string>(queueName, (m) =>
                        {
                            Console.WriteLine(m);
                        });
                    }
                });
                Console.WriteLine("開始接收訊息");
                Console.ReadKey();
            }
        }
    }
}
