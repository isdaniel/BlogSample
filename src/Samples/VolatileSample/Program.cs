using System;
using System.Threading;

namespace VolatileSample
{
    class Program
    {
        static void Main(string[] args)
        {
            int val = Convert.ToInt32(Console.ReadLine());
            Member member = new Member() { Balance = val };

            var t1 = new Thread(member.UpdateBalance);
            t1.Start();

            while (true)
            {
                if (member.Balance <= 0)
                {
                    Console.WriteLine("餘額小於0");
                    break;
                }
              
            }
            Console.WriteLine("執行結束!");
            Console.ReadKey();
        }
    }

    public class Member
    {
        //public volatile int Balance;
        public int Balance;
        public void UpdateBalance()
        {
            Console.WriteLine("開始扣款");
            while (Balance > 0)
            {
                Balance -= 10;
                //Thread.Sleep(200);
            }
            Console.WriteLine($"餘額={Balance}");
        }
    }
}
