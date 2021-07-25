using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VolatileSample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            // Member member = new Member();
            // new Thread(()=>{
                
            //     System.Console.WriteLine($"Sleep 前~ 餘額剩下:{member.balance}");
            //     member.UpdateBalance();
            //     System.Console.WriteLine($"Sleep 結束! 餘額剩下:{member.balance}");
            // }).Start();
            // while (member.balance > 0)
            // {
            //    //等待sub thread把balance改成0跳出迴圈
            // }   
            // Thread.Sleep(50);
            // Console.WriteLine("執行結束!");

            NoAtomicMember m = new NoAtomicMember();
            List<Task> tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(()=>{
                    for (var i = 0; i < 10000; i++)
                    {
                        m.AddBalance();
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            System.Console.WriteLine(m.balance);
            Console.ReadKey();
        }
    }
    public class NoAtomicMember{
          public int balance = 0;
          public void AddBalance(){
            balance+=10;
          }
    }
    public class Member
    {
        public volatile int balance = 100;
        public void UpdateBalance()
        {
            // sub thread update balance to 0
            balance = 0;  
        }
    }
}
