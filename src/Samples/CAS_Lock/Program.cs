using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CAS_Lock
{

    class Program
    {
        static object _lock = new object();
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            int balanceValue = 10000000;
            Member member = new Member() { Balance = balanceValue };
            List<Task> tasks = new List<Task>();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                tasks.Add(Task.Run(() => member.UpdateBalance()));
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine("Lock Version");
            Console.WriteLine($"member remaining balance is {member.Balance}");
            Console.WriteLine($"Exec Time Cost : {sw.ElapsedMilliseconds}");

            tasks.Clear();
            member.Balance = balanceValue;
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                tasks.Add(Task.Run(() => member.UpdateBalanceByInterlock()));
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine("InterLocked Version:");
            Console.WriteLine($"member remaining balance is {member.Balance}");
            Console.WriteLine($"Exec Time Cost : {sw.ElapsedMilliseconds}");

            Console.ReadKey();
        }
    }

    public class Member
    {
        object _lock = new object();

        public int Balance { get; set; }

        public void UpdateBalance()
        {
            lock (_lock)
            {
                Balance -= 10;
            }
        }

        public void UpdateBalanceByInterlock()
        {
            int val = 0;
            Balance = Interlocked.Exchange(ref val, Balance -= 10);
        }
    }
}
