using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeepKnowLock
{
    class Program
    {
        static void Main(string[] args)
        {
            object _object = new object();

            Task.WaitAll(Task.Run(() => { TryLockDemo(_object); }), Task.Run(() => { TryLockDemo(_object); }));
            Console.WriteLine("Hello World!");
        }

        public static void TryLockDemo(object _object) {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"[{threadId}] {DateTime.Now:HH:mm:ss} TryLockDemo Start");
            try
            {
                Monitor.Enter(_object);
                Console.WriteLine($"[{threadId}] {DateTime.Now:HH:mm:ss} get first lock");
                try
                {

                    Monitor.Enter(_object);
                    Console.WriteLine($"[{threadId}] {DateTime.Now:HH:mm:ss} get second lock");
                }
                finally
                {
                    Monitor.Exit(_object);
                    Console.WriteLine($"[{threadId}] {DateTime.Now:HH:mm:ss} release second lock");
                }
            }
            finally
            {
                Monitor.Exit(_object);
                Console.WriteLine($"[{threadId}] {DateTime.Now:HH:mm:ss} release first lock");
            }
        }
    }
}
