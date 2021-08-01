using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace OwnBlockQueue
{
    class Program
    {
        static void Main(string[] args)
        {
 
            OwnBlockQueue<int> data = new OwnBlockQueue<int>(5);
            var t1 = Task.Run(() =>
            {
                data.Add(1);
                Console.WriteLine("add 1");
                data.Add(2);
                Console.WriteLine("add 2");
                data.Add(3);
                Console.WriteLine("add 3");
                data.Add(1);
                Console.WriteLine("add 1");
            });


            var t3 = Task.Run(() =>
            {
                data.Add(11);
                Console.WriteLine("add 11");
                data.Add(22);
                Console.WriteLine("add 22");
                data.Add(33);
                Console.WriteLine("add 33");
            });

            var t2= Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(3000);
                    Console.WriteLine($"data {data.Take()}");
                    if (data.Count == 0)
                    {
                        break;
                        
                    }
                }
                
            });

            Task.WaitAll(t1, t2,t3);
            Console.WriteLine("End");
            Console.ReadKey();
        }
    }

    public class OwnBlockQueue<T>
    {
        private volatile Queue<T> _queue;
        private readonly int _maxLengthLength = 1;
        public OwnBlockQueue(int maxLength)
        {
            _queue = new Queue<T>();
            _maxLengthLength = maxLength;
        }

        public T Take()
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                {
                    Console.WriteLine("等待 Take");
                    Monitor.Wait(_queue);
                }

                var result = _queue.Dequeue();
                Monitor.Pulse(_queue);
                return result;
            }
        }

        public int Count => _queue.Count;

        public void Add(T data)
        {
            lock (_queue)
            {
                while (_queue.Count >= _maxLengthLength)
                {
                    Console.WriteLine($"資料 {data} 等待 Add");
                    Monitor.Wait(_queue);
                }

                Monitor.Pulse(_queue);
            }
            _queue.Enqueue(data);
        }
    }
}
