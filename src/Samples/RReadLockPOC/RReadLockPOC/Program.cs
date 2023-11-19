using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RReadLockPOC
{
    class Program
    {
        

        static void Main(string[] args)
        {
            List<Task> tasks = new List<Task>();

            foreach (var VARIABLE in Enumerable.Range(1, 10))
            {
                ReadLock readLock = new ReadLock("lockkey1");
                ReadLock readLock1 = new ReadLock("lockkey1");
                WriteLock writeLock = new WriteLock("lockkey1");

                tasks.Add(Task.Run(() =>
                {
                    readLock1.TryLock(10);
                    Console.WriteLine($"get readLock1 lock {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                    Thread.Sleep(5000);
                    readLock1.UnLock();
                    Console.WriteLine($"readLock1 release {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                }));

                tasks.Add(Task.Run(() =>
                {
                    writeLock.TryLock(5);
                    Console.WriteLine($"get write lock {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                    Thread.Sleep(2000);
                    writeLock.UnLock();
                    Console.WriteLine($"write lock release! {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                }));


                tasks.Add(Task.Run(() =>
                {
                    readLock.TryLock(10);
                    Console.WriteLine($"get readLock lock {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                    Thread.Sleep(5000);
                    readLock.UnLock();
                    Console.WriteLine($"readLock release {DateTime.Now:yyyy MM dd hh:mm:ss:fff}");
                }));
            }
           


            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("完成");

            Console.ReadKey();
        }

        /*
         *
         *KEYS：
 KEYS1 = anyLock
 
 KEYS[2] = redisson_rwlock:{anyLock}
 
 KEYS[3] = {anyLock}:UUID_01:threadId_01:rwlock_timeout
 
 KEYS[4] = {anyLock}
 
 ARGV[1] = 0
 
 ARGV[2] = UUID_01:threadId_01
         *
         */
    }
}
