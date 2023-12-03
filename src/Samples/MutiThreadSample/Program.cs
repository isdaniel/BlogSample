using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadSample
{
    class Program
    {
        private static int val = 0;
        private static object _rootsync = new object();
        private static void Add(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Thread.Sleep(10);
                ++val;
            }
        }

        private static void LockObjectAdd(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Thread.Sleep(10);
                lock (_rootsync)
                {
                    ++val;
                }
            }
        }

        private static void ThreadSafeAdd(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Thread.Sleep(10);
                Interlocked.Increment(ref val);
            }
        }


        static void Main(string[] args)
        {
            //Interlocked


            //single Thread
            SingleThreadAdd();

            //MultiThread
            MultiThreadAdd();

            MultiThreadLockObject();

            MultiThreadInterlocked();

            Console.ReadKey();
        }

        private static void SingleThreadAdd()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Add(1000);
            sw.Stop();
            Console.WriteLine($"SingleThreadAdd time cost: {sw.ElapsedMilliseconds} value is : {val}");
        }

        private static void MultiThreadInterlocked()
        {
            Stopwatch sw = new Stopwatch();
            List<Task> tasks = new List<Task>();
            val = 0;
            sw.Restart();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => { ThreadSafeAdd(100); }));
            }

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            Console.WriteLine($"MultiThread Interlocked time cost: {sw.ElapsedMilliseconds} value is : {val}");
        }

        private static void MultiThreadLockObject()
        {
            Stopwatch sw = new Stopwatch();
            List<Task> tasks = new List<Task>();
            val = 0;
            sw.Restart();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => { LockObjectAdd(100); }));
            }

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            Console.WriteLine($"MultiThread LockObject time cost: {sw.ElapsedMilliseconds} value is : {val}");
        }

        private static void MultiThreadAdd()
        {
            Stopwatch sw = new Stopwatch();
            List<Task> tasks = new List<Task>();
            val = 0;
            sw.Restart();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => { Add(100); }));
            }

            Task.WaitAll(tasks.ToArray());
            sw.Stop();

            Console.WriteLine($"MultiThreadAdd time cost: {sw.ElapsedMilliseconds} value is : {val}");
        }
    }
}
